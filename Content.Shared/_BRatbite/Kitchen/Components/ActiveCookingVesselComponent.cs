using Content.Shared.Kitchen;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Kitchen.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class ActiveCookingVesselComponent : Component
{
    [ViewVariables]
    public TimeSpan? StopCookingAt;

    [ViewVariables]
    public (FoodRecipePrototype recipe, int portions)? Recipe = null;
}
