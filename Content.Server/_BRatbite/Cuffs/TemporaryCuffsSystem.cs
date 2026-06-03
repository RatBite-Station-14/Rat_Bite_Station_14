// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Cuffs;
using Content.Shared._BRatbite.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.DoAfter;

namespace Content.Server._BRatbite.Cuffs;

public sealed class TemporaryCuffsSystem : EntitySystem
{
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryCuffsComponent, TemporaryCuffsRemovedEvent>(OnCuffsRemoved);
        SubscribeLocalEvent<TemporaryCuffsComponent, TemporaryCuffsStruggleInterruptedEvent>(OnCuffsStruggleInterrupted);
        SubscribeLocalEvent<TemporaryCuffsComponent, TemporaryCuffsBreakoutDoAfterEvent>(OnBreakoutDoAfter);
    }

    private void OnCuffsRemoved(Entity<TemporaryCuffsComponent> ent, ref TemporaryCuffsRemovedEvent args)
    {
        CancelBreakout(ent.Comp);
    }

    private void OnCuffsStruggleInterrupted(Entity<TemporaryCuffsComponent> ent, ref TemporaryCuffsStruggleInterruptedEvent args)
    {
        StartBreakout(ent, args.Target);
    }

    private void StartBreakout(Entity<TemporaryCuffsComponent> ent, EntityUid target)
    {
        CancelBreakout(ent.Comp);

        var doAfter = new DoAfterArgs(EntityManager,
            target,
            ent.Comp.Lifetime,
            new TemporaryCuffsBreakoutDoAfterEvent(),
            ent.Owner,
            target: target,
            used: ent.Owner)
        {
            BreakOnMove = false,
            BreakOnWeightlessMove = false,
            BreakOnDamage = false,
            NeedHand = false,
            RequireCanInteract = false,
            Hidden = false,
            BlockDuplicate = false,
            CancelDuplicate = false,
            DuplicateCondition = DuplicateConditions.SameEvent,
        };

        _doAfter.TryStartDoAfter(doAfter, out ent.Comp.BreakoutDoAfter);
        Log.Debug($"Started temporary cuffs breakout for {ToPrettyString(target)} using {ToPrettyString(ent.Owner)}.");
    }

    private void OnBreakoutDoAfter(Entity<TemporaryCuffsComponent> ent, ref TemporaryCuffsBreakoutDoAfterEvent args)
    {
        ent.Comp.BreakoutDoAfter = null;

        if (args.Cancelled ||
            args.Args.Target is not { } target ||
            !TryComp<CuffableComponent>(target, out var cuffable) ||
            !TryComp<HandcuffComponent>(ent.Owner, out var cuffs) ||
            !IsContained(cuffable, ent.Owner))
        {
            return;
        }

        _cuffable.Uncuff(target, null, ent.Owner, cuffable, cuffs);
    }

    private void CancelBreakout(TemporaryCuffsComponent component)
    {
        _doAfter.Cancel(component.BreakoutDoAfter);
        component.BreakoutDoAfter = null;
    }

    private static bool IsContained(CuffableComponent cuffable, EntityUid cuffs)
    {
        foreach (var contained in cuffable.Container.ContainedEntities)
        {
            if (contained == cuffs)
                return true;
        }

        return false;
    }
}
