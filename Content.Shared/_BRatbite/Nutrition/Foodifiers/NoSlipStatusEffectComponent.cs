namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class NoSlipStatusEffectComponent : Component
{
    [DataField]
    public float SlowDownOverSlipperyMultiplier = 0.7f;
}
