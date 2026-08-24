namespace Content.Shared._BRatbite.Nutrition;

[RegisterComponent]
// A status effect that can be scaled
public sealed partial class StatusEffectScaleComponent : Component
{
    public float Scale = 1f;
}

[ByRefEvent]
public record struct StatusEffectScaleEvent(float NewScale, float OldScale, EntityUid Target);
