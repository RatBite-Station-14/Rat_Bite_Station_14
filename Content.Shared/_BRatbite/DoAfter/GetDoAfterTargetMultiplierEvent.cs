using Content.Shared.DoAfter;

namespace Content.Shared._BRatbite.DoAfter;

[ByRefEvent]
/// Same as <cref>Content.Shared._Shitmed.DoAfter.GetDoAfterDelayMultiplierEvent</cref>
/// But raised on the target instead
public record struct GetDoAfterTargetMultiplierEvent(DoAfterEvent Event, float Multiplier = 1f);
