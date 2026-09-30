using Content.Shared.Mobs;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ChangeMobThresholdStateStatusEffectComponent : Component
{
    [DataField(required: true)]
    public float Amount;

    [DataField(required: true)]
    public MobState State;
}
