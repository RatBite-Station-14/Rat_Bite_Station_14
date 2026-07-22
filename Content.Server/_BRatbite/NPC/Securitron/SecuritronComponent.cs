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

    public EntProtoId CuffsPrototype = "Zipties";
}
