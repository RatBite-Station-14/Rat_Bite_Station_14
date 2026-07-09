using Content.Shared.Humanoid.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Client._BRatbite.Humanoid;

[ByRefEvent]
// Event raised on entities with HumanoidAppearanceComponent before updating the sprite.
public record struct BeforeGetHumanoidAppearanceEvent(ProtoId<SpeciesPrototype> Species, float Height, float Width, Color EyeColor, Color SkinColor);

[ByRefEvent]
// Event raised on entities with HumanoidAppearanceComponent before
// updating markings. If cancelled, markings will not be rendered
public record struct AttemptHumanoidMarkingEvent(bool Cancelled);
