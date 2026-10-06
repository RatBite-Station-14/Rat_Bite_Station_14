namespace Content.Shared._BRatbite.Atmos;

[ByRefEvent]
public partial record struct ExtinguishAttemptEvent(bool Cancelled = false);
