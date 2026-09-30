// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;

namespace Content.Shared._Shitmed.DoAfter;

public sealed class GetDoAfterDelayMultiplierEvent(DoAfterEvent @event, float multiplier = 1f, BodyPartSymmetry? targetBodyPartSymmetry = null) : EntityEventArgs, IBodyPartRelayEvent, IBoneRelayEvent
{
    // Ratbite
    public DoAfterEvent Event = @event;
    // Ratbite end

    public float Multiplier = multiplier;

    public BodyPartType TargetBodyPart => BodyPartType.Hand;

    public BodyPartSymmetry? TargetBodyPartSymmetry => targetBodyPartSymmetry;

    public bool RaiseOnParent => true;
}
