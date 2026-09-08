using Content.Shared.Kitchen;

namespace Content.Shared._BRatbite.Kitchen.Components;

/// <summary>
/// This is used for <see cref="TimedCookerComponent"/>'s currently cooking.
/// </summary>
[RegisterComponent]
public sealed partial class ActiveTimedCookerComponent : Component
{
    [DataField, ViewVariables]
    public (FoodRecipePrototype recipe, int portions)? Recipe;
    [DataField, ViewVariables]
    public TimeSpan EndStamp;
}
