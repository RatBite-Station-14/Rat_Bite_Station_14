using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._BRatbite.Kitchen.Components;
using Robust.Shared.Containers;

namespace Content.Shared._BRatbite.Kitchen.Systems;

/// <summary>
/// Handles containers that cook, like pots.
/// </summary>
public abstract class SharedContainerCookerSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _sharedContainerSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _sharedUserInterface = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ContainerCookerComponent, ContainerCookerEjectIndexedIngredientMessage>(OnEjectIndex);
        SubscribeLocalEvent<ContainerCookerComponent, ContainerCookerEjectMessage>(OnEjectMsg);

        SubscribeLocalEvent<ContainerCookerComponent, EntInsertedIntoContainerMessage>(OnContainerContentsUpdate);
        SubscribeLocalEvent<ContainerCookerComponent, EntRemovedFromContainerMessage>(OnContainerContentsUpdate);
    }

    private void UpdateUserInterfaceState(Entity<CookingVesselComponent> entity)
    {
        _sharedUserInterface.SetUiState(entity.Owner,
            ContainerCookerUiKey.Key,
            new ContainerCookerUpdateUserInterfaceState(entity.Comp.Cooking));
    }

    private void OnEjectMsg(Entity<ContainerCookerComponent> ent, ref ContainerCookerEjectMessage args)
    {
        if (!GetContents(ent, out var cookingVesselComponent))
            return;
        _sharedContainerSystem.EmptyContainer(cookingVesselComponent.Storage);
        UpdateUserInterfaceState((ent, cookingVesselComponent));
    }

    private void OnEjectIndex(Entity<ContainerCookerComponent> ent,
        ref ContainerCookerEjectIndexedIngredientMessage args)
    {
        if (!GetContents(ent, out var cookingVesselComponent))
            return;
        _sharedContainerSystem.Remove(GetEntity(args.EntityId), cookingVesselComponent.Storage);
        UpdateUserInterfaceState((ent, cookingVesselComponent));
    }

    private void OnContainerContentsUpdate(EntityUid ent,
        ContainerCookerComponent containerCookerComponent,
        ContainerModifiedMessage args)
    {
        if (!TryComp<CookingVesselComponent>(ent, out var cookingVesselComponent))
            return;
        UpdateUserInterfaceState((ent, cookingVesselComponent));
    }

    private bool GetContents(EntityUid ent, [NotNullWhen(true)] out CookingVesselComponent? cookingVesselComponent)
    {
        cookingVesselComponent = null;
        if (!TryComp<CookingVesselComponent>(ent, out var definiteComponent))
            return false;
        cookingVesselComponent = definiteComponent;
        return cookingVesselComponent.Storage.ContainedEntities.Any();
    }
}
