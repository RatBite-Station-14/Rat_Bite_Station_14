using Content.Shared.Tools;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.GunTracking;

[RegisterComponent, NetworkedComponent]
public sealed partial class GunTrackableComponent : Component
{
    [DataField]
    public string GunTrackerSlotId = "gun_tracker";

    [DataField]
    public ProtoId<ToolQualityPrototype> QualityToRemove = "Screwing";

    [DataField]
    public TimeSpan RemovalTime = TimeSpan.FromSeconds(25);
}
