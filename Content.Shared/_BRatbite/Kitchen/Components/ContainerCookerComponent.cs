using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Kitchen;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// Used to denote containers that can be used to cook items. <br />
/// Examples include the kitchen cooking pots.
/// <seealso cref="CookingVesselComponent"/>
/// </summary>
[RegisterComponent]
public sealed partial class ContainerCookerComponent : Component
{
    [DataField]
    public SoundSpecifier StartCookingSound = new SoundPathSpecifier("/Audio/Effects/sizzle.ogg");

    /// <summary>
    /// The recipe to make and its portion quantity. <br />
    /// Bigger portions require more time to cook. <br />
    /// Not null when the <see cref="CookingVesselComponent"/>'s contents can satisfy a recipe.
    /// </summary>
    [DataField]
    public (FoodRecipePrototype, int)? Recipe;

    /// <summary>
    /// When the recipe would be finished cooking. <br />
    /// Not null when a recipe is cooking.
    /// </summary>
    [DataField]
    public TimeSpan? EndCookingTime;
}
