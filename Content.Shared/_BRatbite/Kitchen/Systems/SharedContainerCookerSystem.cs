using Content.Shared._BRatbite.Kitchen.Components;
using Robust.Shared.Containers;

namespace Content.Shared._BRatbite.Kitchen.Systems;

/// <summary>
/// Handles containers that cook, like pots.
/// </summary>
public abstract class SharedContainerCookerSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _sharedUserInterface = default!;
    [Dependency] private readonly SharedCookingVesselSystem _sharedCookingVesselSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerCookerComponent, ContainerCookerEjectIndexedIngredientMessage>(OnEjectIndex);
        SubscribeLocalEvent<ContainerCookerComponent, ContainerCookerEjectMessage>(OnEjectMsg);

        SubscribeLocalEvent<ContainerCookerComponent, EntInsertedIntoContainerMessage>((ent, ref _) => UpdateUserInterfaceState(ent));
        SubscribeLocalEvent<ContainerCookerComponent, EntRemovedFromContainerMessage>((ent, ref _) => UpdateUserInterfaceState(ent));
    }

    protected void UpdateUserInterfaceState(EntityUid ent)
    {
        _sharedUserInterface.SetUiState(ent,
            ContainerCookerUiKey.Key,
            new ContainerCookerUpdateUserInterfaceState(HasComp<ActiveCookingVesselComponent>(ent)));
    }

    private void OnEjectMsg(Entity<ContainerCookerComponent> ent, ref ContainerCookerEjectMessage args)
    {
        _sharedCookingVesselSystem.EjectAllContents(ent.Owner);
        UpdateUserInterfaceState(ent);
    }

    private void OnEjectIndex(Entity<ContainerCookerComponent> ent,
        ref ContainerCookerEjectIndexedIngredientMessage args)
    {
        _sharedCookingVesselSystem.EjectEntity(ent.Owner, GetEntity(args.EntityId));
        UpdateUserInterfaceState(ent);
    }
}
