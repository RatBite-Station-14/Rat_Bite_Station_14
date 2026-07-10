// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._White.Standing;
using Content.Shared.Damage.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Traits;

public sealed class MarathonerSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MarathonerComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MarathonerComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MarathonerComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MarathonerComponent, GetStandingUpTimeMultiplierEvent>(OnGetStandingUpTime);
    }

    private void OnStartup(Entity<MarathonerComponent> ent, ref ComponentStartup args)
    {
        AdjustStamina(ent, ent.Comp.StaminaBonus);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnShutdown(Entity<MarathonerComponent> ent, ref ComponentShutdown args)
    {
        AdjustStamina(ent, -ent.Comp.StaminaBonus);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMovementSpeed(Entity<MarathonerComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(ent.Comp.WalkSpeedMultiplier, ent.Comp.SprintSpeedMultiplier);
    }

    private void OnGetStandingUpTime(Entity<MarathonerComponent> ent, ref GetStandingUpTimeMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.StandUpTimeMultiplier;
    }

    private void AdjustStamina(Entity<MarathonerComponent> ent, float delta)
    {
        if (!TryComp<StaminaComponent>(ent, out var stamina))
            return;

        stamina.CritThreshold += delta;
        stamina.NextUpdate = _timing.CurTime;
        Dirty(ent.Owner, stamina);
    }
}
