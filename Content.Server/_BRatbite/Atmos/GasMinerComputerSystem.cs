using Content.Shared._BRatbite.Atmos;
using Content.Shared.UserInterface;
using System.Linq;
using Robust.Server.GameObjects;
using Content.Shared.Atmos.Components;
using Content.Server.Station.Systems;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;
using Content.Server.Atmos.EntitySystems;

namespace Content.Server._BRatbite.Atmos;

public sealed class GasMinerComputerSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly SharedCargoSystem _cargoSystem = default!;
    [Dependency] private readonly GasMinerSystem _gasMinerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasMinerComputerComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);
        SubscribeLocalEvent<GasMinerComputerComponent, GasMinerSetExchangeMessage>(OnGasMinerMessage);
    }

    private void OnBeforeOpened(Entity<GasMinerComputerComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent);
    }

    private void DirtyUI(Entity<GasMinerComputerComponent> ent)
    {
        var station = _stationSystem.GetOwningStation(ent);
        if (station is null || !TryComp<StationBankAccountComponent>(station, out var bankAccount)) return;

        var balance = _cargoSystem.GetBalanceFromAccount((station.Value, bankAccount), ent.Comp.CargoAccount);
        var miners = EntityManager
            .EntityQuery<GasTraderComponent>()
            .Select(c =>
            {
                var owner = c.Owner;
                if (!TryComp<MetaDataComponent>(owner, out var metadata)) return null;
                if (!TryComp<GasMinerComponent>(owner, out var gasMiner)) return null;
                float availableMoles = 0;
                if (_gasMinerSystem.GetValidEnvironment((owner, gasMiner), out var environment))
                {
                    availableMoles = environment.GetMoles(gasMiner.SpawnGas);
                }
                return new GasMinerUIState(metadata.EntityName, "", EntityManager.GetNetEntity(owner), availableMoles, c.SellingState, c.TargetExchangeRate, c.BuyPrice, c.SellPrice, c.MaxExchangeRate);
            }).OfType<GasMinerUIState>().ToArray();

        _userInterfaceSystem.SetUiState(ent.Owner, GasMinerComputerUiKey.Key, new GasMinerComputerBoundUserInterfaceState(miners, balance));
    }


    private void OnGasMinerMessage(Entity<GasMinerComputerComponent> ent, ref GasMinerSetExchangeMessage args)
    {
        var gasMinerUid = EntityManager.GetEntity(args.GasMiner);
        if (!TryComp<GasMinerComponent>(gasMinerUid, out var gasMinerComp)) return;
        if (!TryComp<GasTraderComponent>(gasMinerUid, out var gasTraderComp)) return;
        gasTraderComp.SellingState = args.State;
        gasTraderComp.TargetExchangeRate = Math.Clamp(args.ExchangeRatePerSecond, 0, gasTraderComp.MaxExchangeRate);
        DirtyUI(ent);
    }
}
