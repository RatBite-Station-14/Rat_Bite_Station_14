using System.Numerics;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Silicon;

[RegisterComponent]
public sealed partial class CoreRollerComponent : Component
{
    [DataField]
    public TimeSpan RollDelay = TimeSpan.FromSeconds(2);

    [DataField]
    public float CrushDamage = 150f;

    [DataField]
    public int BonesToBreak = 4;

    [DataField]
    public Vector2 SquishScale = new(1.7f, 0.35f);

    [DataField]
    public TimeSpan SquishDuration = TimeSpan.FromMinutes(5);
}

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class CoreSquishedComponent : Component
{
    [DataField, AutoNetworkedField]
    public Vector2 SquishScale = new(1.7f, 0.35f);

    [DataField, AutoNetworkedField]
    public TimeSpan RecoveryStart;

    [DataField, AutoNetworkedField]
    public TimeSpan RecoveryDuration = TimeSpan.FromMinutes(5);

    [ViewVariables]
    public Vector2? OriginalScale;
}

public sealed partial class CoreRollActionEvent : WorldTargetActionEvent;

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
