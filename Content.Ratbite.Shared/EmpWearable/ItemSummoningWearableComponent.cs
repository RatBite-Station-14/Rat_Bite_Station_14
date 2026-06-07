using Content.Shared.Actions;
using Content.Shared.Inventory;

namespace Content.Ratbite.Shared.EmpGlove;

/// <summary>
/// This is used for...
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ItemSummoningWearableComponent : Component
{
    [DataField]
    public EntProtoId SummonEntity = "TouchEmp";

    public EntityUid? SummonedEntity;

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
