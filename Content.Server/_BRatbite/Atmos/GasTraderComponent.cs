using Content.Shared._BRatbite.Atmos;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Atmos;

[RegisterComponent]
public sealed partial class GasTraderComponent : Component
{
    [DataField]
    public float BuyPrice = 0.0f;

    [DataField]
    public float SellPrice = 0.0f;

    [DataField]
    public GasMinerSellingState SellingState = GasMinerSellingState.None;

    [DataField]
    public float MaxExchangeRate = 50f;

    [DataField]
    public float TargetExchangeRate = 0f;

    [DataField]
    public ProtoId<CargoAccountPrototype> CargoAccount = "Engineering";
}
