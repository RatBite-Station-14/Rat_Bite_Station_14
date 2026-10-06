using Content.Shared.Damage;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class MeleeDamageStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float Multiplier;

    /// <summary>
    /// Extra damage that is dealt with each attack (added after multiplying the damage)
    /// </summary>
    [DataField]
    public DamageSpecifier? ExtraDamage;

    [DataField]
    // Whether or not to buff all melee or only punches
    public bool OnlyPunches;
}
