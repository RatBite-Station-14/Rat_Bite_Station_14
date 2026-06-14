using Robust.Shared.Serialization;
using Content.Shared.UserInterface;
using Content.Shared.Atmos.Components;

namespace Content.Shared._BRatbite.Atmos;

[RegisterComponent]
public sealed partial class GasMinerComputerComponent : Component
{

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
public sealed class GasMinerState
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

    public GasMinerState(string name, string color, NetEntity networkId, float availableMoles, GasMinerSellingState state, float exchangeRatePerSecond, float buyPrice, float sellPrice, float maxExchangeRate)
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
    public readonly GasMinerState[] Miners;
    public readonly float Balance;

    // Temp mock data
    public GasMinerComputerBoundUserInterfaceState()
    {
        // Mock data
        Balance = 1500.75f;
        Miners = new GasMinerState[]
        {
        new GasMinerState(
            name: "Oxygen Miner",
            color: "#4fc3f7",
        networkId: new(1),
        availableMoles: 51f,
            state: GasMinerSellingState.Sell,
            exchangeRatePerSecond: 2.5f,
            buyPrice: 0f,
            sellPrice: 12.50f,
        maxExchangeRate: 30f
        ),
        new GasMinerState(
            name: "Nitrogen Miner",
            color: "#ef5350",
        networkId: new(2),
        availableMoles: 153f,
            state: GasMinerSellingState.Buy,
            exchangeRatePerSecond: 1.0f,
            buyPrice: 8.00f,
            sellPrice: 0f,
                maxExchangeRate: 30f
        ),
        new GasMinerState(
            name: "Plasma Miner",
            color: "#ce93d8",
        networkId: new(3),
        availableMoles: 53f,
            state: GasMinerSellingState.None,
            exchangeRatePerSecond: 0f,
            buyPrice: 0f,
            sellPrice: 0f,
        maxExchangeRate: 30f
        ),
        };
    }

    public GasMinerComputerBoundUserInterfaceState(GasMinerState[] miners, float balance)
    {
        Miners = miners;
        Balance = balance;
    }
}
