using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent, NetworkedComponent]
public sealed partial class InjectOvertimeStatusEffectComponent : Component
{
    [DataField]
    public FixedPoint2 InjectAmountPerSecond = 0.5f;
}
