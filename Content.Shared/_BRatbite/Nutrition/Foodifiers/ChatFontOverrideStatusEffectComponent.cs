namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ChatFontOverrideStatusEffectComponent : Component
{
    [DataField]
    public string? FontId;

    [DataField]
    public Color? Color;
}
