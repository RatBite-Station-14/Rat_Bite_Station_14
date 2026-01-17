using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared.Singularity.Events;

[Serializable, NetSerializable]
public sealed partial class PowerOffDoAfterEvent : SimpleDoAfterEvent;
