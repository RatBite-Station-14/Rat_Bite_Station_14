using Content.Shared.Mobs;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Implants;

[RegisterComponent]
public sealed partial class TeleportOnTriggerComponent : Component
{

    [DataField]
    public EntProtoId MarkerPrototype = "RB_LifelineMarker";

    [DataField("killOnTeleport")]
    public bool KillOnTeleport = true;

    [DataField("allowNukeDisk")]
    public bool AllowNukeDisk = false;
}
