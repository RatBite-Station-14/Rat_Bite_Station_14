using Robust.Shared.Utility;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;
using Content.Shared.DoAfter;

namespace Content.Shared._BRatbite.Medical;

[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState]
public sealed partial class IVComponent : Component
{
    [DataField]
    [AutoNetworkedField]
    public EntityUid? AttachedEntity;

    [DataField]
    public float MaxDistance = 3f;

    [DataField]
    public float InjectedUnitsPerUpdate = 0.5f;

    [DataField]
    public TimeSpan UpdateTime = TimeSpan.FromSeconds(1f);

    [DataField]
    public TimeSpan NextUpdate = TimeSpan.Zero;

    [DataField, ViewVariables]
    public SpriteSpecifier LineSprite =
        new SpriteSpecifier.Rsi(new ResPath("_BRatBites/Objects/Specific/Medical/iv.rsi"), "line");

    [DataField]
    public string ItemSlotId = "ivBag";

    [DataField]
    public string SolutionName = "beaker";
}


[Serializable, NetSerializable]
public sealed partial class IVAttachDoAfterEvent : DoAfterEvent
{
    public NetEntity NewEntity;

    public IVAttachDoAfterEvent(NetEntity newEntity)
    {
        NewEntity = newEntity;
    }

    public override IVAttachDoAfterEvent Clone() => this;
}
