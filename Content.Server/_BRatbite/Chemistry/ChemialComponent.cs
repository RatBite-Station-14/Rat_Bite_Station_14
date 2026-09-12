using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Chemistry;

[RegisterComponent]
public sealed partial class ChemialComponent : Component
{
    [DataField]
    public List<ProtoId<ReagentPrototype>> Reagents = new();

    [DataField]
    public FixedPoint2 SpillQuantity = 0.2f;
}
