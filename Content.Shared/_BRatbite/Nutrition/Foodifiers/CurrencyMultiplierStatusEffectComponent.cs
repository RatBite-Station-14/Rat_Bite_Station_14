namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class CurrencyMultiplierStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float Multiplier = 1f;
}
