using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.CryoSickness;

[RegisterComponent]
public sealed partial class CryoSicknessComponent : Component
{
    [DataField]
    public int DurationInMinutes = 10;

    [DataField]
    public int ExpireSecondsAfterDamage = 30;

    [DataField]
    public EntProtoId Action = "ActionShakeAwake";

    [DataField]
    public EntProtoId Effect = "Pacified";

    [DataField]
    public float DamageResistance = 0.6f;

    [DataField]
    public TimeSpan ExpireTime;

    [DataField]
    public bool HadPacifism = false;

    [DataField]
    public EntityUid? ActionEntity;
}

public sealed partial class ShakeAwakeEvent : InstantActionEvent;
