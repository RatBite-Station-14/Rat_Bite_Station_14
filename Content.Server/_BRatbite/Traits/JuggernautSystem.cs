// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._BRatbite.Traits;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Server._BRatbite.Traits;

public sealed class JuggernautSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThresholds = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JuggernautComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<JuggernautComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnStartup(Entity<JuggernautComponent> ent, ref ComponentStartup args)
    {
        AdjustThreshold(ent, MobState.Critical, ent.Comp.CriticalIncrease);
        AdjustThreshold(ent, MobState.Dead, ent.Comp.DeadIncrease);
    }

    private void OnShutdown(Entity<JuggernautComponent> ent, ref ComponentShutdown args)
    {
        AdjustThreshold(ent, MobState.Critical, -ent.Comp.CriticalIncrease);
        AdjustThreshold(ent, MobState.Dead, -ent.Comp.DeadIncrease);
    }

    private void AdjustThreshold(Entity<JuggernautComponent> ent, MobState state, int delta)
    {
        if (!_mobThresholds.TryGetThresholdForState(ent, state, out var current))
            return;

        _mobThresholds.SetMobStateThreshold(ent, FixedPoint2.Max(0, current.Value + delta), state);
    }
}
