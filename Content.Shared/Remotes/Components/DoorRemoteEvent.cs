// Ratbite refactor: Use events instead of hardcoding

namespace Content.Shared.Remotes.Components;

[ByRefEvent]
public record struct DoorRemoteUsedEvent(EntityUid Target, EntityUid User, Entity<DoorRemoteComponent> Remote, OperatingMode Mode, EntityUid AccessTarget, bool Handled = false);
