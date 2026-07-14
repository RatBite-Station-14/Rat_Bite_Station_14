using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Actions;
using Content.Shared.StatusEffect;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.CryoSickness;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CryoSicknessComponent : Component
{
    [DataField]
    public int DurationInMinutes = 10;

    [DataField]
    public int ExpireSecondsAfterDamage = 30;

    [DataField]
    public EntProtoId Action = "ActionShakeAwake";

    [DataField]
    public ProtoId<StatusEffectPrototype> Effect = "Pacified";

    [DataField]
    public float DamageResistance = 0.6f;

    [DataField, AutoNetworkedField]
    public TimeSpan ExpireTime;

    [DataField, AutoNetworkedField]
    public bool HadPacifism = false;

    [DataField]
    public EntityUid? ActionEntity;

    [DataField]
    // Minimum damage per hit before the sickness is removed.
    // Keep in mind, this is with the damage resistance already applied
    // So it's actually 10
    public FixedPoint2 MinDamageBeforeRemove = 4;

    [DataField]
    // Minimum total damage before the sickness is removed
    public FixedPoint2 MinTotalDamageBeforeRemove = 15;
}

public sealed partial class ShakeAwakeEvent : InstantActionEvent;
