using Robust.Shared.Serialization;
using Content.Shared.UserInterface;
using Content.Shared.Atmos.Components;
using Content.Shared.Cargo.Prototypes;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Atmos;

[RegisterComponent]
public sealed partial class GasMinerComputerComponent : Component
{
    [DataField]
    public ProtoId<CargoAccountPrototype> CargoAccount;
}

[Serializable, NetSerializable]
public enum GasMinerComputerUiKey
{
    Key
}

[Serializable, NetSerializable]
public enum GasMinerSellingState : byte
{
    None,
    Buy,
    Sell,
}

[Serializable, NetSerializable]
public sealed class GasMinerUIState
{
    public readonly string Name;
    public readonly string Color;
    public readonly NetEntity NetworkId;
    public readonly float AvailableMoles;
    public readonly GasMinerSellingState State;
    // How many moles are we buying or selling per second
    public readonly float ExchangeRatePerSecond;
    public readonly float BuyPrice;
    public readonly float SellPrice;
    public readonly float MaxExchangeRate;

    public GasMinerUIState(string name, string color, NetEntity networkId, float availableMoles, GasMinerSellingState state, float exchangeRatePerSecond, float buyPrice, float sellPrice, float maxExchangeRate)
    {
        Name = name;
        Color = color;
        NetworkId = networkId;
        AvailableMoles = availableMoles;
        State = state;
        ExchangeRatePerSecond = exchangeRatePerSecond;
        BuyPrice = buyPrice;
        SellPrice = sellPrice;
        MaxExchangeRate = maxExchangeRate;
    }
}

[Serializable]
[NetSerializable]
public sealed class GasMinerComputerBoundUserInterfaceState : BoundUserInterfaceState
{
    public readonly GasMinerUIState[] Miners;
    public readonly int Balance;

    public GasMinerComputerBoundUserInterfaceState(GasMinerUIState[] miners, int balance)
    {
        Miners = miners;
        Balance = balance;
    }
}

[Serializable, NetSerializable]
public sealed class GasMinerSetExchangeMessage : BoundUserInterfaceMessage
{
    public NetEntity GasMiner;
    public float ExchangeRatePerSecond;
    public GasMinerSellingState State;

    public GasMinerSetExchangeMessage(NetEntity gasMiner, float exchangeRatePerSecond, GasMinerSellingState state)
    {
        GasMiner = gasMiner;
        ExchangeRatePerSecond = exchangeRatePerSecond;
        State = state;
    }
}

