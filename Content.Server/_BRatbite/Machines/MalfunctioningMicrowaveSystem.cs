using Content.Server.Administration.Logs;
using Content.Server.Construction.Components;
using Content.Server.Explosion.EntitySystems;
using Content.Server.Lightning;
using Content.Shared.Database;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Machines;

/// <summary>
/// This handles...
/// </summary>
public sealed class MalfunctioningMicrowaveSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly ExplosionSystem _explosion = default!;
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly LightningSystem _lightning = default!;

    private static readonly EntProtoId MalfunctionSpark = "Spark";

    /// <inheritdoc/>
    public override void Initialize()
    {
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeMicrowaves = EntityQueryEnumerator<MalfunctioningMicrowaveComponent>();
        while (activeMicrowaves.MoveNext(out var uid, out var malfunctioningMicrowaveComponent))
        {
            if (malfunctioningMicrowaveComponent.NextMalfunction > _gameTiming.CurTime)
                continue;
            malfunctioningMicrowaveComponent.NextMalfunction =
                _gameTiming.CurTime.Add(TimeSpan.FromSeconds(malfunctioningMicrowaveComponent.MalfunctionInterval));

            if (_random.Prob(malfunctioningMicrowaveComponent.ExplosionChance))
            {
                if (TryComp<MachineComponent>(uid, out var machine))
                {
                    _container.CleanContainer(machine.BoardContainer);
                    _container.CleanContainer(machine.PartContainer);
                }

                _explosion.TriggerExplosive(uid);


                _adminLogger.Add(LogType.Action,
                    LogImpact.Medium,
                    $"{ToPrettyString(uid)} exploded from unsafe cooking!");

                continue; // microwave should be destroyed
            }

            if (_random.Prob(malfunctioningMicrowaveComponent.LightningChance))
                _lightning.ShootRandomLightnings(uid, 1.0f, 2, MalfunctionSpark, triggerLightningEvents: false);
        }
    }
}
