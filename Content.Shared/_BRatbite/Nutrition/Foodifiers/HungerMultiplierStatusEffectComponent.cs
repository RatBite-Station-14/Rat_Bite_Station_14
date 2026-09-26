namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class HungerMultiplierStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float Multiplier;
}
