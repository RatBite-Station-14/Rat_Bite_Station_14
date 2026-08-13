using System.Threading;
using System.Threading.Tasks;
using Content.Server._BRatbite.NPC.Securitron;
using Content.Server.NPC;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Server.NPC.Pathfinding;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Interaction;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Stealth.Components;

namespace Content.Server._BRatbite.NPC.HTN;

public sealed partial class PickSecuritronTargetOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private EntityLookupSystem _lookup;
    private PathfindingSystem _pathfinding;
    private SharedCuffableSystem _cuffableSystem;
    private SecuritronSystem _securitronSystem;

    [DataField]
    public string TargetCoordinatesKey = "TargetCoordinates";

    [DataField]
    public string TargetKey = "Target";

    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _lookup = sysManager.GetEntitySystem<EntityLookupSystem>();
        _pathfinding = sysManager.GetEntitySystem<PathfindingSystem>();
        _cuffableSystem = sysManager.GetEntitySystem<SharedCuffableSystem>();
        _securitronSystem = sysManager.GetEntitySystem<SecuritronSystem>();
    }

    public override async Task<(bool Valid, Dictionary<string, object>? Effects)> Plan(NPCBlackboard blackboard,
    CancellationToken cancelToken)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);

        if (!blackboard.TryGetValue<float>(NPCBlackboard.SecuritronRange, out var range, _entManager))
            return (false, null);
        if (!_entManager.TryGetComponent<SecuritronComponent>(owner, out var securitron)) return (false, null);
        var mobState = _entManager.GetEntityQuery<MobStateComponent>();
        var stealthQuery = _entManager.GetEntityQuery<StealthComponent>();
        var cuffableQuery = _entManager.GetEntityQuery<CuffableComponent>();

        (EntityUid, PathResultEvent)? selectedEntity = null;
        int maxThreatLevel = -1;

        foreach (var entity in _lookup.GetEntitiesInRange(owner, range))
        {
            if (!mobState.TryGetComponent(entity, out var state) || state.CurrentState != MobState.Alive) continue;
            if (stealthQuery.TryGetComponent(entity, out var stealth) && stealth.Enabled) continue;
            if (!cuffableQuery.TryGetComponent(entity, out var cuffable) || _cuffableSystem.IsCuffed((entity, cuffable))) continue;

            var threat = _securitronSystem.GetTargetThreatLevel((owner, securitron), entity);
            // Do the threat check now so we don't have to compute the path for every entity
            if (threat < securitron.MinThreatLevel || threat <= maxThreatLevel) continue;

            var pathRange = SharedInteractionSystem.InteractionRange - 1f;
            var path = await _pathfinding.GetPath(owner, entity, pathRange, cancelToken);
            if (path.Result == PathResult.NoPath) continue;
            // We already did the threat check before, we know this is the highest target found so far
            maxThreatLevel = threat;
            selectedEntity = (entity, path);
        }

        if (selectedEntity is (var e, var p))
        {
            return (true, new Dictionary<string, object>{
                    {TargetCoordinatesKey, _entManager.GetComponent<TransformComponent>(e).Coordinates},
                    {TargetKey, e},
                    {NPCBlackboard.PathfindKey, p}
                });
        }

        return (false, null);
    }
}
