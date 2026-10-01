using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, AutoGenerateComponentPause, Access(typeof(SharedCookedFoodSystem))]
public sealed partial class CookedFoodComponent : Component
{
    [DataField]
    public ProtoId<FoodTemperaturePrototype> FoodTemperaturePrototype = "HotFoodTemperature";

    [DataField, ViewVariables(VVAccess.ReadWrite)]
    public ProtoId<FoodDecayPrototype> FoodDecayPrototype = "HotFoodDecay";

    [ViewVariables, AutoNetworkedField]
    // Elapsed time, accounting for food temperature.
    // May not be updated, use SharedCookedFoodSystem::GetFreshnessLevel
    public TimeSpan ElapsedTime;

    [ViewVariables, AutoPausedField, AutoNetworkedField]
    // Elapsed time since last decay update
    public TimeSpan LastFoodDecayUpdate;

    [DataField("freshness")]
    // Use to set the starting freshness of the food
    // Do not use to query the current freshness, as it may not be updated
    // Use SharedCookedFoodSystem::GetFreshnessLevel instead
    public ProtoId<FoodStatusPrototype> CurrentFreshness = "Fresh";

    [DataField]
    // These are the status effects inherent to the food itself or the
    // way it was cooked.
    public List<EntProtoId> StatusEffectProto = new();
}
