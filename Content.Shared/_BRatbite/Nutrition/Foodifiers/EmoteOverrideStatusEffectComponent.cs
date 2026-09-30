using Content.Shared.Chat.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class EmoteOverrideStatusEffectComponent : Component
{
    [DataField(required: true)]
    public ProtoId<EmoteSoundsPrototype> Sounds;
}
