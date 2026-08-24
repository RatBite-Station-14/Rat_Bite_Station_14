using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition;

[RegisterComponent, Access(typeof(SharedCookedFoodSystem))]
public sealed partial class CookedFoodComponent : Component
{
    [DataField]
    public FoodType FoodType = FoodType.HotFood;

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FoodDecayPrototype> FoodDecayPrototype = "HotFoodDecay";

    [ViewVariables]
    // The last time when the freshness was updated
    public TimeSpan LastFreshnessUpdate;

    [DataField("freshness")]
    // Use to set the starting freshness of the food
    // Do not use to query the current freshness, as it may not be updated
    // Use SharedCookedFoodSystem::GetFreshnessLevel instead
    public ProtoId<FoodFreshnessPrototype> CurrentFreshness = "Fresh";

    [DataField]
    // These are the status effects inherent to the food itself.
    public List<EntProtoId> StatusEffectProto = new();
}

[Flags]
/// <summary> 
///    How foods will react to specific conditions like heat.
///    For example, Cold foods should be eaten cold, while hot foods
///    should be eaten hot.
///    TODO: switch to a prototype
/// </summary>
public enum FoodType : byte
{
    HotFood = 1 << 0,
    ColdFood = 1 << 1,
}

