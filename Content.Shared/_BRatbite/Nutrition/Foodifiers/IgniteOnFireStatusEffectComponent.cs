namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

/// <summary>
/// Ignites the entity on fire (and can keep them on fire too)
/// </summary>
[RegisterComponent]
public sealed partial class IgniteOnFireStatusEffectComponent : Component
{
    /// <summary>
    /// How much fire stacks to add to the entity when they get the effect
    /// </summary>
    [DataField]
    public float FireStacksToAdd;

    /// <summary>
    /// How many of those fire stacks never go away
    /// </summary>
    [DataField]
    public float GuaranteedFireStacks;
}
