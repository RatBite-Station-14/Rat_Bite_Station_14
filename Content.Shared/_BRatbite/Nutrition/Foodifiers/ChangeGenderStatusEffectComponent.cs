using Robust.Shared.Enums;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ChangeGenderStatusEffectComponent : Component
{
    [DataField]
    public Gender NewGender = Gender.Epicene;
}
