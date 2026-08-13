using Content.Shared.Humanoid.Prototypes;
using Content.Shared.Inventory;
using Content.Shared.Roles;
using Content.Shared.Security;
using Content.Shared.StatusIcon;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.NPC.Securitron;

[RegisterComponent]
public sealed partial class SecuritronComponent : Component
{
    // Min threat level before engaging
    [DataField]
    public int MinThreatLevel = 4;

    [DataField]
    public EntProtoId WeaponPrototype = "SecuritronStunbaton";

    [ViewVariables]
    public EntityUid? Stunbaton;

    [ViewVariables]
    public EntProtoId CuffsPrototype = "Zipties";

    [DataField]
    // All other species will have +1 threat level
    public ProtoId<SpeciesPrototype> BiasSpecies = "Human";

    [ViewVariables]
    // Last entity that attacked us, if any
    public EntityUid? AttackerEntity;

    [DataField]
    public int BiasSpeciesThreatLevel = 1;

    [DataField]
    public int AttackedThreatLevel = 6;

    [DataField]
    public Dictionary<SecurityStatus, int> WantedThreatLevels = new Dictionary<SecurityStatus, int>{
        {SecurityStatus.Suspected, 2},
        {SecurityStatus.Wanted, 5},
        {SecurityStatus.Search, 2},
        {SecurityStatus.Perma, 2},
        {SecurityStatus.Dangerous, 6}
    };

    [DataField]
    public Dictionary<ProtoId<DepartmentPrototype>, int> DepartmentThreatLevels = new Dictionary<ProtoId<DepartmentPrototype>, int> {
        {"CentralCommand", -100},
        {"Command", -2},
        {"Security", -2},
        {"Science", 1}
    };

    [DataField]
    public int UnknownThreatLevel = 2;

    [DataField]
    public int ContrabandThreatLevel = 1;

    [DataField]
    public int AgentIdThreatLevel = -5;

    [DataField]
    public SlotFlags SlotsToCheck = SlotFlags.WITHOUT_POCKET;
}
