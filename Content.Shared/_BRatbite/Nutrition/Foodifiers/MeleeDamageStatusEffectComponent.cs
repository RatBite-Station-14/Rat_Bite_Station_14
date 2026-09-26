namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class MeleeDamageStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float Multiplier;

    [DataField]
    // Whether or not to buff all melee or only punches
    public bool OnlyPunches;
}
