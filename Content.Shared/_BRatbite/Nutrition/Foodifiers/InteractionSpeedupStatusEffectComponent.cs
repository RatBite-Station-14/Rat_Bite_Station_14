namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

// Changes the speed of certain interactions performed by the entity
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

[RegisterComponent]
public sealed partial class HealingInteractionSpeedupStatusEffectComponent : InteractionSpeedupStatusEffectComponent;

// Changes the speed of certain interactions when performed to the entity
public abstract partial class InteractedSpeedupStatusEffectComponent : Component
{
    [DataField]
    public float Multiplier = 0.2f;
}

[RegisterComponent]
public sealed partial class SurgeryInteractedSpeedupStatusEffectComponent : InteractedSpeedupStatusEffectComponent;

[RegisterComponent]
public sealed partial class InjectionInteractedSpeedupStatusEffectComponent : InteractedSpeedupStatusEffectComponent;

[RegisterComponent]
public sealed partial class HealingInteractedSpeedupStatusEffectComponent : InteractedSpeedupStatusEffectComponent;
