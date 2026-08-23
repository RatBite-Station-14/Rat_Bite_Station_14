using Content.Server.Physics.Controllers;
using Content.Server.Tesla.Components;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Tesla;

public sealed partial class TeslaRandomSystem : EntitySystem
{
    [Dependency] private readonly ChasingWalkSystem _chasingWalkSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TeslaEnergyBallComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(Entity<TeslaEnergyBallComponent> ent, ref MapInitEvent args)
    {
        _chasingWalkSystem.SetReverse(ent.Owner, _random.Prob(0.5f));
    }
}
