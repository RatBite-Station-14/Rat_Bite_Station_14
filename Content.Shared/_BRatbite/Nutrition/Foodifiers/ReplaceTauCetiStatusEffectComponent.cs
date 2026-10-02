using Content.Shared._EinsteinEngines.Language;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ReplaceTauCetiStatusEffectComponent : Component
{
    [DataField(required: true)]
    public ProtoId<LanguagePrototype> Language;

    [DataField]
    public bool RemoveUnderstanding = false;
}
