using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TakeDamageOvertimeStatusEffectComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public DamageSpecifier DamagePerSecond = new();
}
