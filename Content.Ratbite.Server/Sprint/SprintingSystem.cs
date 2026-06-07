using Content.Ratbite.Shared.Sprinting;
using Content.Server.Stunnable;
using Robust.Shared.Physics.Events;

namespace Content.Ratbite.Server.Sprint;

public sealed partial class SprintingSystem : SharedSprintingSystem
{

    [Dependency] private StunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SprinterComponent, StartCollideEvent>(OnCollide);
    }

    private void OnCollide(EntityUid uid, SprinterComponent sprinter, ref StartCollideEvent args)
    {
        var otherUid = args.OtherEntity;

        if (uid.Id < otherUid.Id)
            return;

        if (!sprinter.IsSprinting || !TryComp(otherUid, out SprinterComponent? otherSprinter) || !otherSprinter.IsSprinting)
            return;

        _stun.TryKnockdown(uid, sprinter.KnockdownDurationOnInterrupt, false, drop: false);
        _stun.TryKnockdown(otherUid, otherSprinter.KnockdownDurationOnInterrupt, false, drop: false);
    }
}
