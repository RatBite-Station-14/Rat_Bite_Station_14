using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Silicon;

[RegisterComponent]
public sealed partial class CoreRollerComponent : Component
{
    [DataField]
    public TimeSpan RollDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public float CrushDamage = 100f;

    [DataField]
    public EntProtoId ControlAction = "ActionToggleAiCoreControl";

    [DataField]
    public EntityUid? ControlActionEntity;
}

public sealed partial class CoreRollActionEvent : WorldTargetActionEvent;

public sealed partial class ToggleAiCoreControlActionEvent : InstantActionEvent;

[Serializable, NetSerializable]
public sealed partial class CoreRollDoAfterEvent : SimpleDoAfterEvent
{
    [DataField(required: true)]
    public NetCoordinates Destination;

    private CoreRollDoAfterEvent()
    {
    }

    public CoreRollDoAfterEvent(NetCoordinates destination)
    {
        Destination = destination;
    }

    public override DoAfterEvent Clone() => this;
}
