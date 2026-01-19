using Content.Server.Emp;
using Content.Shared._BRatbite.EmpGlove;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._BRatbite.EmpWearable;

/// <summary>
/// This handles spawning in items for wearables.
/// Functions very similarly to mansus grasp, but can take any entity.
/// </summary>
public sealed class ItemSummoningWearableSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    [Dependency] private readonly SharedActionsSystem _actionsSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSummoningWearableComponent, ItemSummonActionEvent>(OnSummonEmp);
        SubscribeLocalEvent<ItemSummoningWearableComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ItemSummoningWearableComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<ItemSummoningWearableComponent> ent, ref MapInitEvent args)
    {
        _actionContainer.EnsureAction(ent, ref ent.Comp.ActionEntity, ent.Comp.Action);
        Dirty(ent);
    }

    private void OnGetActions(Entity<ItemSummoningWearableComponent> ent, ref GetItemActionsEvent args)
    {
        if (!args.SlotFlags.HasValue)
            return;

        if (!ent.Comp.ValidSlots.HasFlag(args.SlotFlags))
            return;

        args.AddAction(ent.Comp.ActionEntity);
    }

    private void OnSummonEmp(Entity<ItemSummoningWearableComponent> ent, ref ItemSummonActionEvent args)
    {
        if (ent.Comp.SummonedEntity.HasValue)
        {
            QueueDel(ent.Comp.SummonedEntity);
            ent.Comp.SummonedEntity = null;
            _actionsSystem.SetToggled(args.Action, false);
            return;
        }

        if (!_hands.TryGetEmptyHand(args.Performer, out var emptyHand))
            return;

        var summonedItem = Spawn(ent.Comp.SummonEntity, Transform(ent).Coordinates);

        if (!_hands.TryPickup(args.Performer, summonedItem, emptyHand, animate: false))
        {
            QueueDel(summonedItem);
            return;
        }

        ent.Comp.SummonedEntity = summonedItem;
        _actionsSystem.SetToggled(args.Action, true);

        if (!TryComp<SummonedItemDespawnComponent>(summonedItem, out var summonedItemComp))
            return;

        summonedItemComp.Creator = ent;
    }
}
