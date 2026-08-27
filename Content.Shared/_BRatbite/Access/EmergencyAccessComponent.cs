using Content.Shared.Access;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Access;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(false, true)]
public sealed partial class EmergencyAccessComponent : Component
{

    // Tags that are added when the target alert is reached
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<AccessLevelPrototype>> AddedTags = new();
    // Tags that are removed when the target alert is reached
    [DataField, AutoNetworkedField]
    public HashSet<ProtoId<AccessLevelPrototype>> RemovedTags = new();

    // Access Groups. These are added to the tags during map init. After map init this will have no effect.
    [DataField(readOnly: true)]
    [AutoNetworkedField]
    public HashSet<ProtoId<AccessGroupPrototype>> AddedGroups = new();

    // These are used to populate RemovedTags during map init
    [DataField(readOnly: true)]
    [AutoNetworkedField]
    public HashSet<ProtoId<AccessGroupPrototype>> RemovedGroups = new();

    [DataField]
    public string TargetAlert = "red";

    [DataField, AutoNetworkedField]
    public string CurrentAlertLevel = ""; // This is used for prediction
                                          // because the client doesn't know
                                          // about alert levels
}
