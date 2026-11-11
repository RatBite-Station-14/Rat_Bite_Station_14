// This creative work/file is freely and wholly released into the public domain by its original owner/creator, Fishied, under CC0 1.0: https://creativecommons.org/publicdomain/zero/1.0

using Content.Server._BRatbite.Spawners;
using Content.Server.StationEvents.Events;
using Content.Server.Storage.EntitySystems;
using Content.Shared.GameTicking.Components;
using Content.Shared.Station.Components;
using Content.Shared.Storage.Components;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.StationEvents;

public sealed class StowawayClosetSpawnRule : StationEventSystem<StowawayClosetSpawnRuleComponent>
{
    [Dependency] private readonly EntityStorageSystem _entityStorage = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;


    protected override void Started(EntityUid ruleUid,
        StowawayClosetSpawnRuleComponent stowawayRule,
        GameRuleComponent gameRule,
        GameRuleStartedEvent startedEv)
    {
        base.Started(ruleUid, stowawayRule, gameRule, startedEv);
        if (!TryGetRandomStation(out var targetStation)) return; 

        var stowaway = Spawn(stowawayRule.MobToSpawn, MapCoordinates.Nullspace);

        if (TryStuffInCloset(stowaway, targetStation))
            return;
        if (TryDumpOnRandomTile(stowaway, targetStation))
            return;
        Log.Warning($"Couldn't find a stowaway closet OR a station tile for {ToPrettyString(stowaway)}, binning it.");
        Del(stowaway);
    }

    private bool TryStuffInCloset(EntityUid stowaway, EntityUid? targetStation)
    {
        var candidateClosets = new List<(EntityUid, EntityStorageComponent)>();
        var closetQuery = EntityQueryEnumerator<StowawaySpawnPointComponent, EntityStorageComponent, TransformComponent>();

        while (closetQuery.MoveNext(out var closetUid, out var spawnPoint, out var closetStorage, out var closetXform))
        {
            if (spawnPoint.Secure || closetStorage.Open
                || StationSystem.GetOwningStation(closetUid, closetXform) != targetStation)
            {
                continue;
            }

            if (_entityStorage.CanInsert(stowaway, closetUid, closetStorage)) candidateClosets.Add((closetUid, closetStorage));
        }
        while (candidateClosets.Count > 0)
        {
            var (pickedCloset, pickedStorage) = RobustRandom.PickAndTake(candidateClosets);
            if (_entityStorage.Insert(stowaway, pickedCloset, pickedStorage))
                return true;
        }
        return false;
    }

    private bool TryDumpOnRandomTile(EntityUid stowaway, EntityUid? targetStation)
    {
        if (targetStation is not { } stationUid
            || !TryComp<StationDataComponent>(stationUid, out var stationData))
            return false;

        if (!TryFindRandomTileOnStation((stationUid, stationData), out _, out _, out var fallbackSpot))
            return false;

        _transform.SetCoordinates(stowaway, fallbackSpot);
        return true;
    }
}
