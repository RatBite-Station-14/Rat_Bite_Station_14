namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
// Additional description in case it can't be inferred from the components
public sealed partial class EffectDescriptionStatusEffectComponent : Component
{
    [DataField]
    public LocId Description;
}
