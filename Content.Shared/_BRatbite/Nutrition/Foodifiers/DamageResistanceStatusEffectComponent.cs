using Content.Shared.Damage;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class DamageResistanceStatusEffectComponent : Component
{
    [DataField(required: true)]
    public DamageModifierSet DamageModifier = new();
}
