namespace Content.Ratbite.Shared.Sprinting;

[Serializable, NetSerializable]
public sealed class SprintToggleEvent(bool isSprinting) : EntityEventArgs
{
    public bool IsSprinting = isSprinting;
}

[Serializable, NetSerializable]
public sealed class SprintStartEvent : EntityEventArgs;

[ByRefEvent]
public sealed class SprintAttemptEvent : CancellableEntityEventArgs;
