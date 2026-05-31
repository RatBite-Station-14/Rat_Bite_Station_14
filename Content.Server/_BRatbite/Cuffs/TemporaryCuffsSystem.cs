// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Cuffs;
using Content.Shared._BRatbite.Cuffs;
using Content.Shared.Cuffs.Components;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Cuffs;

public sealed class TemporaryCuffsSystem : EntitySystem
{
    [Dependency] private readonly CuffableSystem _cuffable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    private readonly List<PendingTemporaryCuffs> _pending = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TemporaryCuffsComponent, TemporaryCuffsRemovedEvent>(OnCuffsRemoved);
        SubscribeLocalEvent<TemporaryCuffsComponent, TemporaryCuffsStruggleInterruptedEvent>(OnCuffsStruggleInterrupted);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            var pending = _pending[i];

            if (pending.BreakTime > _timing.CurTime)
                continue;

            _pending.RemoveAt(i);

            if (!TryComp<CuffableComponent>(pending.Target, out var cuffable) ||
                !TryComp<HandcuffComponent>(pending.Cuffs, out var cuffs) ||
                !IsContained(cuffable, pending.Cuffs))
            {
                continue;
            }

            _cuffable.Uncuff(pending.Target, null, pending.Cuffs, cuffable, cuffs);
        }
    }

    private void OnCuffsRemoved(Entity<TemporaryCuffsComponent> ent, ref TemporaryCuffsRemovedEvent args)
    {
        RemovePending(ent.Owner);
    }

    private void OnCuffsStruggleInterrupted(Entity<TemporaryCuffsComponent> ent, ref TemporaryCuffsStruggleInterruptedEvent args)
    {
        RemovePending(ent.Owner);
        _pending.Add(new PendingTemporaryCuffs(ent.Owner, args.Target, _timing.CurTime + ent.Comp.Lifetime));
    }

    private void RemovePending(EntityUid cuffs)
    {
        for (var i = _pending.Count - 1; i >= 0; i--)
        {
            if (_pending[i].Cuffs == cuffs)
                _pending.RemoveAt(i);
        }
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

    private readonly record struct PendingTemporaryCuffs(EntityUid Cuffs, EntityUid Target, TimeSpan BreakTime);
}
