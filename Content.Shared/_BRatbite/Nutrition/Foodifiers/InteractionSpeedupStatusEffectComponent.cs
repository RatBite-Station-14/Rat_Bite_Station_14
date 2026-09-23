using Content.Shared.DoAfter;
using static Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public abstract partial class InteractionSpeedupStatusEffectComponent : Component
{
    [DataField]
    public float Multiplier = 0.2f;
}

[RegisterComponent]
public sealed partial class ToolInteractionSpeedupStatusEffectComponent : InteractionSpeedupStatusEffectComponent;

[RegisterComponent]
public sealed partial class SurgeryInteractionSpeedupStatusEffectComponent : InteractionSpeedupStatusEffectComponent;

[RegisterComponent]
public sealed partial class InjectionInteractionSpeedupStatusEffectComponent : InteractionSpeedupStatusEffectComponent;
