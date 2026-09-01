using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.GunTracking;

[RegisterComponent, NetworkedComponent]
public sealed partial class GunTrackerComponent : Component
{ }

[RegisterComponent, NetworkedComponent]
public sealed partial class GunTrackingComputerComponent : Component;

[Serializable, NetSerializable]
public enum GunTrackingUIKey
{
    Key
}

[Serializable, NetSerializable]
public sealed partial class GunTrackingComputerState : BoundUserInterfaceState
{
    public List<GunStatus> Guns;

    public GunTrackingComputerState(List<GunStatus> guns)
    {
        Guns = guns;
    }
}

[Serializable, NetSerializable]
public sealed partial class GunStatus
{
    public NetEntity Uid;
    public string Name;
    public NetCoordinates Coordinates;

    public GunStatus(NetEntity uid, string name, NetCoordinates coordinates)
    {
        Uid = uid;
        Name = name;
        Coordinates = coordinates;
    }
}

[Serializable, NetSerializable]
public sealed partial class RemoveTrackerEvent : SimpleDoAfterEvent;
