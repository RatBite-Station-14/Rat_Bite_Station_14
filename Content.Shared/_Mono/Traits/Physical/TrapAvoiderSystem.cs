// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Cancels step triggers for entities that have TrapAvoiderComponent.
/// </summary>
public sealed class TrapAvoiderSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<StepTriggerComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
    }

    private void OnStepTriggerAttempt(Entity<StepTriggerComponent> ent, ref StepTriggerAttemptEvent args)
    {
        if (HasComp<TrapAvoiderComponent>(args.Tripper))
            args.Cancelled = true;
    }
}
