using Content.Goobstation.Common.Standing;

namespace Content.Goobstation.Common.Stunnable;

[ByRefEvent]
public record struct BeforeStunEvent(
    bool Cancelled);

[ByRefEvent]
// Ratbite: Add override for behavior
public record struct BeforeKnockdownEvent(DropHeldItemsBehavior Behavior,
    bool Cancelled);

[ByRefEvent]
public record struct BeforeTrySlowdownEvent(
    bool Cancelled);
