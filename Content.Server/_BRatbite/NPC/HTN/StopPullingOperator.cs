using Content.Server.NPC;
using Content.Server.NPC.HTN;
using Content.Server.NPC.HTN.PrimitiveTasks;
using Content.Shared.Movement.Pulling.Systems;

namespace Content.Server._BRatbite.NPC.HTN;

public sealed partial class StopPullingOperator : HTNOperator
{
    [Dependency] private readonly IEntityManager _entManager = default!;
    private PullingSystem _pullingSystem;
    public override void Initialize(IEntitySystemManager sysManager)
    {
        base.Initialize(sysManager);
        _pullingSystem = sysManager.GetEntitySystem<PullingSystem>();
    }

    public override HTNOperatorStatus Update(NPCBlackboard blackboard, float frameTime)
    {
        var owner = blackboard.GetValue<EntityUid>(NPCBlackboard.Owner);
        _pullingSystem.StopAllPulls(owner, stopPullable: false, stopPuller: true);

        return HTNOperatorStatus.Finished;
    }
}
