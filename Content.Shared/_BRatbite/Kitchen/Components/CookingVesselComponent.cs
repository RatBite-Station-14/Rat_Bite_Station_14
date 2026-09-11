using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Item;
using Robust.Shared.Containers;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Used to denote things that can cook. See <see cref="SharedCookingVesselSystem"/>
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState, Access(typeof(SharedCookingVesselSystem))]
public sealed partial class CookingVesselComponent : Component
{
    [DataField("failureResult")]
    public EntProtoId BadRecipeEntityId = "FoodBadRecipe";

    [DataField, AutoNetworkedField, ViewVariables]
    public ProtoId<FoodPreparationMethodPrototype> PreparationMethod = "Microwaving";

    [DataField, AutoNetworkedField, ViewVariables]
    public bool RequiresPower;

    #region storage
    public Container Storage = default!;

    [DataField, AutoNetworkedField]
    public string ContainerId = "cooking_entity_container";

    [DataField, AutoNetworkedField]
    public int ItemCapacity = 10;

    [DataField, AutoNetworkedField]
    public ProtoId<ItemSizePrototype> MaxItemSize = "Normal";
    #endregion
}
