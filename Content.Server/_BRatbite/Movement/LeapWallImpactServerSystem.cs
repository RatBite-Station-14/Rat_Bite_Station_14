using Content.Shared._BRatbite.Movement;

namespace Content.Server._BRatbite.Movement;

public sealed class LeapWallImpactServerSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LeapWallImpactEvent>(OnLeapWallImpact);
    }

    private void OnLeapWallImpact(LeapWallImpactEvent ev)
    {
        RaiseNetworkEvent(ev, GetEntity(ev.Entity));
    }
}
