using Robust.Shared.GameStates;

namespace Content.Ratbite.Shared.Anger.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class AdjustAngerOnHitComponent : Component
{
    [DataField]
    public float AdjustAngerOnAttack = 0.1f;
}
