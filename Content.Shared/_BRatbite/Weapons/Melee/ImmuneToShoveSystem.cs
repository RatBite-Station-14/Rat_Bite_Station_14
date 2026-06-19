using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Mobs;

namespace Content.Shared._BRatbite.Weapons.Melee;

public sealed class ImmuneToShoveSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ImmuneToShoveOnDeathComponent, MobStateChangedEvent>(OnMobStateChange);
        SubscribeLocalEvent<ImmuneToShoveOnDeathComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnMobStateChange(Entity<ImmuneToShoveOnDeathComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead)
        {
            EnsureComp<ImmuneToShoveComponent>(ent.Owner);

        }
        else
        {
            RemComp<ImmuneToShoveComponent>(ent.Owner);
        }
    }

    private void OnShutdown(Entity<ImmuneToShoveOnDeathComponent> ent, ref ComponentShutdown args)
    {
        RemComp<ImmuneToShoveComponent>(ent.Owner);
    }


}
