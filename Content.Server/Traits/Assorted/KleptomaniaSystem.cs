// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Shared.ActionBlocker;
using Content.Shared.Ghost;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Strip;
using Content.Shared.Strip.Components;
using Robust.Shared.Containers;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Random;

namespace Content.Server.Traits.Assorted;

public sealed class KleptomaniaSystem : EntitySystem
{
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedStrippableSystem _strippable = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedContainerSystem _containers = default!;

    private static readonly SlotFlags NormalSlots =
        SlotFlags.HEAD | SlotFlags.EYES | SlotFlags.MASK | SlotFlags.NECK | SlotFlags.GLOVES;

    private static readonly SlotFlags RiskyVisibleSlots = NormalSlots |
        SlotFlags.EARS |
        SlotFlags.OUTERCLOTHING |
        SlotFlags.INNERCLOTHING |
        SlotFlags.BELT |
        SlotFlags.IDCARD |
        SlotFlags.LEGS |
        SlotFlags.FEET;

    public override void Initialize()
    {
        SubscribeLocalEvent<KleptomaniaComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<KleptomaniaComponent> ent, ref ComponentStartup args)
    {
        ent.Comp.NextIncidentTime = GetNextInterval(ent.Comp);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<KleptomaniaComponent, HandsComponent>();
        while (query.MoveNext(out var uid, out var klepto, out var hands))
        {
            klepto.NextIncidentTime -= frameTime;
            if (klepto.NextIncidentTime > 0f)
                continue;

            klepto.NextIncidentTime += GetNextInterval(klepto);

            if (!_actionBlocker.CanInteract(uid, null) ||
                !_hands.TryGetEmptyHand((uid, hands), out _))
                continue;

            if (TryPickupLooseItem((uid, klepto, hands)))
            {
                klepto.NextIncidentTime = MathF.Max(klepto.NextIncidentTime, klepto.FloorItemCooldown);
                continue;
            }

            TryStartStripAttempt((uid, klepto, hands));
        }
    }

    private float GetNextInterval(KleptomaniaComponent component)
    {
        var min = MathF.Min(component.TimeBetweenIncidents.X, component.TimeBetweenIncidents.Y);
        var max = MathF.Max(component.TimeBetweenIncidents.X, component.TimeBetweenIncidents.Y);
        return min.Equals(max) ? max : _random.NextFloat(min, max);
    }

    private bool TryPickupLooseItem(Entity<KleptomaniaComponent, HandsComponent> ent)
    {
        var nearby = _lookup.GetEntitiesInRange(Transform(ent).Coordinates,
            ent.Comp1.Range,
            LookupFlags.Dynamic | LookupFlags.Sundries).ToList();
        _random.Shuffle(nearby);

        foreach (var item in nearby)
        {
            if (item == ent.Owner ||
                !HasComp<ItemComponent>(item) ||
                _containers.IsEntityInContainer(item))
                continue;

            if (TryComp<TransformComponent>(item, out var xform) && xform.Anchored)
                continue;

            if (TryComp<PhysicsComponent>(item, out var physics) && physics.BodyType == BodyType.Static)
                continue;

            if (!_interaction.InRangeAndAccessible(ent.Owner, item, ent.Comp1.Range))
                continue;

            if (_hands.TryPickupAnyHand(ent.Owner, item, handsComp: ent.Comp2))
                return true;
        }

        return false;
    }

    private bool TryStartStripAttempt(Entity<KleptomaniaComponent, HandsComponent> ent)
    {
        var targets = _lookup.GetEntitiesInRange<StrippableComponent>(Transform(ent).Coordinates,
            ent.Comp1.Range,
            LookupFlags.Dynamic).ToList();
        _random.Shuffle(targets);

        foreach (var (target, strippable) in targets)
        {
            if (!CanStealFrom(ent.Owner, target, ent.Comp1.Range))
                continue;

            if (TryStealFromHand((ent.Owner, ent.Comp2), (target, null), strippable))
                return true;

            if (TryStealFromInventory(ent.Owner, target, strippable, ent.Comp1.RiskyTargets))
                return true;
        }

        return false;
    }

    private bool CanStealFrom(EntityUid user, EntityUid target, float range)
    {
        if (target == user ||
            HasComp<GhostComponent>(target) ||
            !_interaction.InRangeAndAccessible(user, target, range))
            return false;

        if (TryComp<MobStateComponent>(target, out var mobState) &&
            (_mobState.IsCritical(target, mobState) || _mobState.IsDead(target, mobState)))
            return false;

        return true;
    }

    private bool TryStealFromHand(
        Entity<HandsComponent?> user,
        Entity<HandsComponent?> target,
        StrippableComponent strippable)
    {
        if (!Resolve(target, ref target.Comp, false))
            return false;

        var hands = _hands.EnumerateHands(target).ToList();
        _random.Shuffle(hands);

        foreach (var hand in hands)
        {
            if (!_hands.TryGetHeldItem(target, hand, out var item) ||
                item is not { } held)
                continue;

            if (_strippable.TryStartStripRemoveHand(user, target, held, hand, strippable))
                return true;
        }

        return false;
    }

    private bool TryStealFromInventory(EntityUid user, EntityUid target, StrippableComponent strippable, bool risky)
    {
        if (!TryComp<InventoryComponent>(target, out var inventory))
            return false;

        var flags = risky ? RiskyVisibleSlots : NormalSlots;
        var slots = _inventory.GetSlotEnumerator((target, inventory), flags);
        var options = new List<(EntityUid Item, string Slot)>();

        while (slots.NextItem(out var item, out var slot))
        {
            if (slot == null)
                continue;

            options.Add((item, slot.Name));
        }

        _random.Shuffle(options);

        foreach (var (item, slot) in options)
        {
            if (_strippable.TryStartStripRemoveInventory(user, target, item, slot))
                return true;
        }

        return false;
    }
}
