using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Server.Spawners.Components;
using Content.Server.Station.Components;
using Content.Server.StationEvents.Components;
using Content.Server.StationEvents.Events;
using Content.Shared.GameTicking.Components;
using Content.Shared.Roles;
using System.Linq;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Content.Server.Mind;
using Content.Server.Station.Systems;

namespace Content.Server._BRatbite.GameTicking.Rules;

public sealed class ReinforcementRuleSystem : StationEventSystem<ReinforcementRuleComponent>
{
    [Dependency] private readonly ArrivalsSystem _arrivalsSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly MindSystem _mindSystem = default!;
    [Dependency] private readonly StationJobsSystem _stationJobsSystem = default!;

    protected override void Added(EntityUid uid, ReinforcementRuleComponent comp, GameRuleComponent rule, GameRuleAddedEvent args)
    {
        if (!TryComp<StationEventComponent>(uid, out var stationEvent))
            return;
        if (!TryGetRandomStation(out var station, (station) => HasComp<StationArrivalsComponent>(station))) return;
        if (!_arrivalsSystem.TryGetArrivals(out var arrivals)) return;
        if (!TryComp<TransformComponent>(arrivals, out var arrivalsXform))
            return;
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out var _, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType != SpawnPointType.LateJoin || xform.MapID != arrivalsXform.MapID)
                continue;
            possiblePositions.Add(xform.Coordinates);
        }
        if (possiblePositions.Count == 0) return;
        var spawnWeight = new Dictionary<ProtoId<DepartmentPrototype>, float>();
        foreach (var (key, value) in comp.ReinforcementsPrototypes)
        {
            if (!comp.PrioritizeUnderstaffedDepartments)
            {
                // Give everything their weight modifier
                spawnWeight.Add(key, value.WeightModifier);
                continue;
            }
            var department = _proto.Index(key);
            var freeJobs = _stationJobsSystem.GetJobs(station.Value)
            .Where(j => department.Roles.Contains(j.Key))
            .Select(j => (j.Value != -1 && j.Value is { } count) ? count : 0)
            .Sum();
            // Add one to give a small chance to spawn full staffed departments too
            spawnWeight.Add(key, (freeJobs + 1) * value.WeightModifier);
        }

        var spawnedDictionary = new Dictionary<string, int>();
        var toSpawn = _random.Next(comp.MinReinforcementsToSpawn, comp.MaxReinforcementsToSpawn + 1);

        for (int i = 0; i < toSpawn; i++)
        {
            if (spawnWeight.Count == 0) break;
            var totalWeight = spawnWeight.Values.Sum();
            var roll = _random.Next(totalWeight);
            var picked = spawnWeight.Keys.First();
            var accumulator = 0f;
            foreach (var (item, weight) in spawnWeight)
            {
                accumulator += weight;
                if (roll < accumulator)
                {
                    picked = item;
                    if (--spawnWeight[item] <= 0)
                    {
                        spawnWeight.Remove(item);
                    }
                    break;
                }
            }
            var position = _random.Pick(possiblePositions);
            var randPrototype = _random.Pick(comp.ReinforcementsPrototypes[picked].Prototypes);
            Spawn(randPrototype, position);
            spawnedDictionary[picked] = spawnedDictionary.GetValueOrDefault(picked, 0) + 1;
        }
        stationEvent.StartAnnouncement = Loc.GetString("reinforcement-rule-announcement", [
                                   ("reinforcements", String.Join(", ", spawnedDictionary.Select(s=>Loc.GetString("reinforcement-rule-department", [("count", s.Value), ("department", s.Key)])))),
        ]);

        base.Added(uid, comp, rule, args);
    }
}
