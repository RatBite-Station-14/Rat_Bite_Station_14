using Content.Shared.Kitchen;

namespace Content.Shared._BRatbite.Kitchen.Components;

[RegisterComponent]
public sealed partial class ActiveCookingVesselComponent : Component
{
    [ViewVariables]
    public TimeSpan? StopCookingAt;

    [ViewVariables]
    public (FoodRecipePrototype recipe, int portions)? Recipe = null;
}
