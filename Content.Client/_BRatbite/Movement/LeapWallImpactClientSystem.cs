using Content.Shared._BRatbite.Movement;
using Content.Shared.Stunnable;

namespace Content.Client._BRatbite.Movement;

public sealed class LeapWallImpactClientSystem : EntitySystem
{
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeNetworkEvent<LeapWallImpactEvent>(OnLeapWallImpact);
    }

    private void OnLeapWallImpact(LeapWallImpactEvent ev)
    {
        var entity = GetEntity(ev.Entity);
        _stun.TryKnockdown(entity, ev.KnockdownDuration, force: true);
    }
}
