using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ChangeStaminaStatusEffectComponent : Component
{
    [DataField, AutoNetworkedField]
    public float AddedStamina = 5f;
}
