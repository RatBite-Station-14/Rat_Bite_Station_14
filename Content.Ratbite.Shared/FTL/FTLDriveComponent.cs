using Content.Shared.Shuttles.Systems;

namespace Content.Ratbite.Shared.FTL;

/// <summary>
/// Assigned to shuttles that are able to FTL.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FTLDriveComponent : Component
{
    [DataField, AutoNetworkedField]
    public FTLDriveData Data = new(SharedShuttleSystem.FTLRange, false);
}
