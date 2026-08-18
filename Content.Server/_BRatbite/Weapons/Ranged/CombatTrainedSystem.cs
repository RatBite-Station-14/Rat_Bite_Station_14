using Content.Goobstation.Common.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Events;
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
        SubscribeLocalEvent<CombatTrainedComponent, ComponentStartup>(OnInitializeComp);
        SubscribeLocalEvent<CombatTrainedComponent, ComponentShutdown>(OnShutdownComp);
        SubscribeLocalEvent<NukeOperativeComponent, ComponentStartup>(OnInitializeNukeOps);
    }

    private void OnInitializeNukeOps(Entity<NukeOperativeComponent> ent, ref ComponentStartup args)
    {
        // Add it like this to nukeops because there are a bunch of nuclear operative prototypes
        // And I don't want to add them manually
        _entityManager.RemoveComponent<CombatUntrainedComponent>(ent.Owner);
        AddComp<CombatTrainedComponent>(ent.Owner);
    }

    private void OnInitializeComp(Entity<CombatTrainedComponent> ent, ref ComponentStartup args)
    {
        _entityManager.RemoveComponent<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnShutdownComp(Entity<CombatTrainedComponent> ent, ref ComponentShutdown args)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

        AddComp<CombatUntrainedComponent>(ent.Owner);
    }

    private void OnRecoilModifiersEvent(Entity<CombatUntrainedComponent> ent, ref GetRecoilModifiersEvent args)
    {
        args.Modifier = (args.Modifier * ent.Comp.RecoilDebuff) + ent.Comp.FlatRecoilDebuff;
    }

}
