using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.TrackingHud.MarkerMonitor;

[RegisterComponent, NetworkedComponent]
public sealed partial class MarkerMonitorComputerComponent : Component;

[Serializable, NetSerializable]
public enum MarkerMonitorUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class MarkerBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly List<TrackingTarget> Targets;

    public MarkerBoundUserInterfaceState(List<TrackingTarget> targets)
    {
        Targets = targets;
    }
}

[Serializable, NetSerializable]
public sealed partial class AddMarkerBoundUserInterfaceMessage : BoundUserInterfaceMessage
{
    public readonly Vector2 TargetLocation;
    public readonly ProtoId<MarkerPrototype> Marker;
    public readonly Color Color;

    public AddMarkerBoundUserInterfaceMessage(Vector2 targetLocation, ProtoId<MarkerPrototype> marker, Color color)
    {
        TargetLocation = targetLocation;
        Marker = marker;
        Color = color;
    }
}
