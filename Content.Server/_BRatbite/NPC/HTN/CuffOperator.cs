using Content.Server._BRatbite.NPC.Securitron;
using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;

namespace Content.Server._BRatbite.NPC.HTN;

public sealed partial class CuffOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private SecuritronSystem _securitronSystem;

    [DataField]
    public string TargetKey = "Target";


    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _securitronSystem = sysManager.GetEntitySystem<SecuritronSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        if (!blackboard.TryGetValue<EntityUid>(TargetKey, out var target, _entManager)) return HTNOperatorStatus.Failed;
        // We intentionally don't get near the stunned guy others can
        // drag people away and fail the interaction
        if (!_securitronSystem.TryCuffAndPull(owner, target)) return HTNOperatorStatus.Failed;
        return HTNOperatorStatus.Finished;
    }
}
