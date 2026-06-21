using Content.Server.Atmos.EntitySystems;
using Content.Server.Atmos.Piping.Components;
using Content.Server.Cargo.Systems;
using Content.Server.Station.Systems;
using Content.Shared._BRatbite.Atmos;
using Content.Shared.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Cargo;
using Content.Shared.Cargo.Components;

namespace Content.Server._BRatbite.Atmos;

public sealed partial class GasTraderSystem : EntitySystem
{
    [Dependency] private readonly GasMinerSystem _gasMinerSystem = default!;
    [Dependency] private readonly StationSystem _stationSystem = default!;
    [Dependency] private readonly CargoSystem _cargoSystem = default!;
    [Dependency] private readonly AtmosphereSystem _atmosphereSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GasTraderComponent, AtmosDeviceUpdateEvent>(OnAtmoUpdate);
    }

    private void OnAtmoUpdate(Entity<GasTraderComponent> ent, ref AtmosDeviceUpdateEvent args)
    {
        if (ent.Comp.SellingState == GasMinerSellingState.None) return;
        if (!Transform(ent).Anchored) return;
        if (!TryComp<GasMinerComponent>(ent, out var gasMinerComponent)) return;
        if (!_gasMinerSystem.GetValidEnvironment((ent.Owner, gasMinerComponent), out var environment)) return;
        var station = _stationSystem.GetOwningStation(ent);
        if (station is null || !TryComp<StationBankAccountComponent>(station, out var bankAccount)) return;

        var balance = _cargoSystem.GetBalanceFromAccount((station.Value, bankAccount), ent.Comp.CargoAccount);
        if (ent.Comp.SellingState == GasMinerSellingState.Buy)
        {
            // Get max amount according to atmos system
            var maxSpawnAtmo = _gasMinerSystem.CapSpawnAmount((ent, gasMinerComponent), ent.Comp.TargetExchangeRate * args.dt, environment);
            var maxCanBuy = ent.Comp.BuyPrice == 0 ? float.PositiveInfinity : balance / ent.Comp.BuyPrice;
            var merger = new GasMixture(1) { Temperature = gasMinerComponent.SpawnTemperature };
            var molesBought = MathF.Min(maxCanBuy, maxSpawnAtmo);
            merger.SetMoles(gasMinerComponent.SpawnGas, molesBought);
            _atmosphereSystem.Merge(environment, merger);
            _cargoSystem.UpdateBankAccount((station.Value, bankAccount), -(int) MathF.Ceiling(molesBought * ent.Comp.BuyPrice), ent.Comp.CargoAccount);
        }
        else
        {
            var moles = environment.GetMoles(gasMinerComponent.SpawnGas);
            var targetSell = ent.Comp.TargetExchangeRate * args.dt;
            var molesSold = MathF.Min(moles, targetSell);
            environment.AdjustMoles(gasMinerComponent.SpawnGas, -molesSold);
            _cargoSystem.UpdateBankAccount((station.Value, bankAccount), (int) MathF.Floor(molesSold * ent.Comp.SellPrice), ent.Comp.CargoAccount);

        }
    }
}
