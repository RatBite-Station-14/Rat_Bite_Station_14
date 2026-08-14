using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Armor;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MindshieldArmorComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Slowdown = 0.75f;
}
