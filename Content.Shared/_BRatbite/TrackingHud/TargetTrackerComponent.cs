using System.Numerics;
using Content.Shared._BRatbite.TrackingHud.MarkerMonitor;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.TrackingHud;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedTrackingTargetSystem))]
public sealed partial class TargetTrackerComponent : Component
{
    // Don't spam other clients with this
    public override bool SendOnlyToOwner => true;

    [ViewVariables, AutoNetworkedField]
    public Dictionary<string, TrackingTarget> Targets = new();

    [DataField]
    public ListeningChannels Channels = ListeningChannels.SECURITY;
}

[Serializable, NetSerializable, Flags]
public enum ListeningChannels : byte
{
    NONE = 0,
    SECURITY = 1 << 0,
    SILICON = 1 << 1,
    CARGO = 1 << 2,
    ENGINEERING = 1 << 3,
    MEDICAL = 1 << 4,
    SCIENCE = 1 << 5,

    BSO = SECURITY | CARGO | ENGINEERING | MEDICAL | SCIENCE // All but silicon
}

[Serializable, NetSerializable, DataDefinition]
public partial class TrackingTarget
{
    [DataField]
    public Vector2 TargetLocation;

    [DataField]
    public Color PinColor = new Color(255, 0, 0, 180);

    [DataField]
    public MapId MapId;

    [DataField]
    public ProtoId<MarkerPrototype> MarkerPrototype = "WarningExclamation";

    [DataField]
    public ListeningChannels Channels = ListeningChannels.SECURITY;
}
