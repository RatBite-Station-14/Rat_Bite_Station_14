using System.Numerics;
using Content.Server.Administration.Logs;
using Content.Server.Nuke;
using Content.Server.Singularity.Events;
using Content.Shared.Database;
using Content.Shared.Ghost;
using Content.Shared.Mind.Components;
using Content.Shared.Singularity.Components;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Nuke;

/// <summary>
/// Handles <see cref="NukeAnnihilationComponent"/>, an expanding bubble spawned when a nuke detonates that
/// instantly deletes everything caught inside its (growing) radius, ahead of the normal explosion.
/// </summary>
public sealed class NukeAnnihilationSystem : EntitySystem
{
    [Dependency] private readonly IAdminLogManager _adminLogger = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IMapManager _mapMan = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedTransformSystem _xform = default!;

    private const float BubbleMaxRadius = 20f;
    private const float BubbleExpansionRate = 6f;

    // dummy event horizon state used purely to reuse existing event-horizon immunity checks
    // (e.g. EventHorizonIgnoreComponent) via EventHorizonAttemptConsumeEntityEvent, without
    // depending on the AGPL singularity system directly.
    private readonly EventHorizonComponent _dummyEventHorizon = new();

    private EntityQuery<MapGridComponent> _gridQuery;
    private EntityQuery<MapComponent> _mapQuery;

    public override void Initialize()
    {
        base.Initialize();

        _gridQuery = GetEntityQuery<MapGridComponent>();
        _mapQuery = GetEntityQuery<MapComponent>();

        SubscribeLocalEvent<NukeExplodedEvent>(OnNukeExploded);
    }

    private void OnNukeExploded(NukeExplodedEvent ev)
    {
        if (!TryComp<TransformComponent>(ev.Nuke, out var xform))
            return;

        SpawnBubble(_xform.GetMapCoordinates(ev.Nuke, xform), BubbleMaxRadius, BubbleExpansionRate);
    }

    /// <summary>
    /// Spawns an annihilation bubble at the given coordinates, expanding towards <paramref name="maxRadius"/>
    /// at <paramref name="expansionRate"/> tiles per second.
    /// </summary>
    public EntityUid SpawnBubble(MapCoordinates coordinates, float maxRadius, float expansionRate)
    {
        var uid = Spawn(null, coordinates);
        var comp = AddComp<NukeAnnihilationComponent>(uid);
        comp.MaxRadius = maxRadius;
        comp.ExpansionRate = expansionRate;
        comp.NextExpandTime = _timing.CurTime;
        return uid;
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var curTime = _timing.CurTime;
        var query = EntityQueryEnumerator<NukeAnnihilationComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var comp, out var xform))
        {
            if (curTime < comp.NextExpandTime)
                continue;

            comp.NextExpandTime = curTime + comp.ExpandInterval;
            comp.CurrentRadius = Math.Min(comp.MaxRadius,
                comp.CurrentRadius + comp.ExpansionRate * (float) comp.ExpandInterval.TotalSeconds);

            var mapPos = _xform.GetMapCoordinates(uid, xform);
            foreach (var ent in _lookup.GetEntitiesInRange(mapPos, comp.CurrentRadius, LookupFlags.Uncontained))
            {
                if (ent == uid || EntityManager.IsQueuedForDeletion(ent))
                    continue;

                // don't delete the station/map itself, just what's on it
                if (_gridQuery.HasComp(ent) || _mapQuery.HasComp(ent))
                    continue;

                // ghosts and anything explicitly immune to event horizon-style deletion should survive
                if (IsImmune(ent, uid))
                    continue;

                if (HasComp<MindContainerComponent>(ent))
                    _adminLogger.Add(LogType.EntityDelete, LogImpact.High, $"{ToPrettyString(ent):player} was annihilated by a nuclear detonation");

                QueueDel(ent);
            }

            ConsumeTiles(uid, mapPos, comp.CurrentRadius);

            if (comp.CurrentRadius >= comp.MaxRadius)
                QueueDel(uid);
        }
    }

    /// <summary>
    /// Turns every grid tile within the bubble's current radius into space, unless something immune to the
    /// bubble is anchored to it.
    /// </summary>
    private void ConsumeTiles(EntityUid bubble, MapCoordinates mapPos, float radius)
    {
        var box = Box2.CenteredAround(mapPos.Position, new Vector2(radius * 2, radius * 2));
        var grids = new List<Entity<MapGridComponent>>();
        _mapMan.FindGridsIntersecting(mapPos.MapId, box, ref grids);

        var circle = new Circle(mapPos.Position, radius);
        foreach (var grid in grids)
        {
            var toClear = new List<(Vector2i, Tile)>();
            foreach (var tile in _mapSystem.GetTilesIntersecting(grid.Owner, grid.Comp, circle))
            {
                if (tile.Tile.IsEmpty)
                    continue;

                var blocked = false;
                foreach (var anchored in grid.Comp.GetAnchoredEntities(tile.GridIndices))
                {
                    if (IsImmune(anchored, bubble))
                    {
                        blocked = true;
                        break;
                    }
                }

                if (!blocked)
                    toClear.Add((tile.GridIndices, Tile.Empty));
            }

            if (toClear.Count > 0)
                _mapSystem.SetTiles(grid.Owner, grid.Comp, toClear);
        }
    }

    /// <summary>
    /// Whether an entity should be spared from the annihilation bubble. Ghosts are always immune, and anything
    /// else gets to react the same way it would to a real event horizon (e.g. EventHorizonIgnoreComponent).
    /// </summary>
    private bool IsImmune(EntityUid ent, EntityUid bubble)
    {
        if (HasComp<GhostComponent>(ent))
            return true;

        var ev = new EventHorizonAttemptConsumeEntityEvent(ent, bubble, _dummyEventHorizon);
        RaiseLocalEvent(ent, ref ev);
        return ev.Cancelled;
    }
}
