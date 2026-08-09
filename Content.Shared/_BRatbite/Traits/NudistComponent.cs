using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class NudistComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<SlotFlags, ClothingModifier> ClothingModifier = new();

    [DataField, AutoNetworkedField]
    public ClothingModifier BaseModifier = new(1.2f, 1.15f);

    [DataField, AutoNetworkedField]
    public ClothingModifier CachedModifier = new(1f, 1f);
}


[Serializable, DataDefinition]
public partial struct ClothingModifier
{
    [DataField]
    public float SpeedModifier;

    [DataField]
    public float StaminaModifier;

    public ClothingModifier(float speedModifier, float staminaModifier)
    {
        SpeedModifier = speedModifier;
        StaminaModifier = staminaModifier;
    }
}
