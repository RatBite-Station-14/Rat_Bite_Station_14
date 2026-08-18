using Content.Goobstation.Common.Standing;
using Content.Goobstation.Common.Stunnable;

namespace Content.Shared._BRatbite.Weapons.Melee;

public sealed class FirmGripSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FirmGripComponent, BeforeKnockdownEvent>(OnBeforeKnockdown);
    }

    public void OnBeforeKnockdown(Entity<FirmGripComponent> ent, ref BeforeKnockdownEvent args)
    {
        args.Behavior = DropHeldItemsBehavior.NoDrop;
    }
}
