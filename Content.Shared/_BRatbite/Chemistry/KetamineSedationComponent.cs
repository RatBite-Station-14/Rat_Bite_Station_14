using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Chemistry;

[RegisterComponent, NetworkedComponent]
public sealed partial class KetamineSedationComponent : Component
{
    [DataField]
    public float DoAfterDelayMultiplier = 2f;
}
