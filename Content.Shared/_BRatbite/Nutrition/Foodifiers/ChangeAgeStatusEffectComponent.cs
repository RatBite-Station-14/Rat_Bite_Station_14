namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ChangeAgeStatusEffectComponent : Component
{
    [DataField(required: true)]
    public int Amount;
}
