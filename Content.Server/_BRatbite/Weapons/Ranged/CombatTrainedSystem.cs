using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared._BRatbite.Weapons.Ranged;
using Content.Shared.NukeOps;

namespace Content.Server._BRatbite.Weapons.Ranged;

public sealed partial class CombatTrainedSystem : EntitySystem
{
    [Dependency] private readonly IEntityManager _entityManager = default!;


    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<CombatUntrainedComponent, GetRecoilModifiersEvent>(OnRecoilModifiersEvent);
        SubscribeLocalEvent<CombatTrainedComponent, MapInitEvent>(OnInitializeComp);
        SubscribeLocalEvent<CombatTrainedComponent, ComponentShutdown>(OnShutdownComp);
        SubscribeLocalEvent<NukeOperativeComponent, MapInitEvent>(OnInitializeNukeOps);
    }

    private void OnInitializeNukeOps(Entity<NukeOperativeComponent> ent, ref MapInitEvent args)
    {
        // Add it like this to nukeops because there are a bunch of nuclear operative prototypes
        // And I don't want to add them manually
        RemComp<CombatUntrainedComponent>(ent.Owner);
        EnsureComp<CombatTrainedComponent>(ent.Owner);
    }

    private void OnInitializeComp(Entity<CombatTrainedComponent> ent, ref MapInitEvent args)
    {
        if (LifeStage(ent) >= EntityLifeStage.Terminating) return;
        RemComp<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnShutdownComp(Entity<CombatTrainedComponent> ent, ref ComponentShutdown args)
    {
        if (LifeStage(ent) >= EntityLifeStage.Terminating) return;
        EnsureComp<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnRecoilModifiersEvent(Entity<CombatUntrainedComponent> ent, ref GetRecoilModifiersEvent args)
    {
        args.Modifier = (args.Modifier * ent.Comp.RecoilDebuff) + ent.Comp.FlatRecoilDebuff;
    }
}
