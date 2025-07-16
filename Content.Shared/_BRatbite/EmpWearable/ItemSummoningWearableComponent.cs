using Content.Shared.Actions;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.TypeSerializers.Implementations.Custom.Prototype;

namespace Content.Shared._BRatbite.EmpGlove;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemSummoningWearableComponent : Component
{
    [DataField(customTypeSerializer: typeof(PrototypeIdSerializer<EntityPrototype>))]
    public string SummonEntity = "TouchEmp";

    public EntityUid SummonedEntity = EntityUid.Invalid;

    [DataField]
    public EntProtoId Action = "ActionEmpSpawn";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionEntity;

    [DataField]
    public TimeSpan CooldownAfterUse = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public SlotFlags ValidSlots = SlotFlags.GLOVES;
}

public sealed partial class ItemSummonActionEvent : InstantActionEvent;
