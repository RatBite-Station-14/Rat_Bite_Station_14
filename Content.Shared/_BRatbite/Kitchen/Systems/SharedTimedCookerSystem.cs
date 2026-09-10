using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Shared._BRatbite.Kitchen.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._BRatbite.Kitchen.Systems;

/// <summary>
/// This handles timed cooking machines. <see cref="TimedCookerComponent"/>
/// </summary>
public abstract class SharedTimedCookerSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _sharedUserInterface = default!;
    [Dependency] private readonly SharedContainerSystem _sharedContainerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TimedCookerComponent, EntInsertedIntoContainerMessage>(OnContainerContentsUpdate);
        SubscribeLocalEvent<TimedCookerComponent, EntRemovedFromContainerMessage>(OnContainerContentsUpdate);
        SubscribeLocalEvent<TimedCookerComponent, TimedCookerEjectMessage>(OnEjectMsg);
        SubscribeLocalEvent<TimedCookerComponent, TimedCookerEjectIndexedIngredientMessage>(OnEjectIndex);

        SubscribeLocalEvent<ActiveTimedCookerComponent, EntInsertedIntoContainerMessage>(OnActiveCookerInsert);
        SubscribeLocalEvent<ActiveTimedCookerComponent, EntRemovedFromContainerMessage>(OnActiveCookerRemove);

        SubscribeLocalEvent<TimedCookerComponent, OpenBoundInterfaceMessage>(OnUIOpen);
        SubscribeLocalEvent<TimedCookerComponent, CloseBoundInterfaceMessage>(OnUIClose);
    }

    private void OnUIOpen(Entity<TimedCookerComponent> ent, ref OpenBoundInterfaceMessage args)
    {
        if (ent.Comp.OnUIOpenSound is not null)
            _sharedAudioSystem.PlayPredicted(ent.Comp.OnUIOpenSound, ent, args.Actor, AudioParams.Default);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.DoorOpen, true);
    }

    private void OnUIClose(Entity<TimedCookerComponent> ent, ref CloseBoundInterfaceMessage args)
    {
        if (ent.Comp.OnUICloseSound is not null)
            _sharedAudioSystem.PlayPredicted(ent.Comp.OnUICloseSound, ent, args.Actor, AudioParams.Default);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.DoorOpen, false);
    }

    private void OnActiveCookerRemove(Entity<ActiveTimedCookerComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (HasComp<BeingUsedInRecipeComponent>(args.Entity))
            RemCompDeferred<BeingUsedInRecipeComponent>(args.Entity);
    }

    private void OnActiveCookerInsert(Entity<ActiveTimedCookerComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        var beingUsedInRecipeComponent = AddComp<BeingUsedInRecipeComponent>(args.Entity);
        beingUsedInRecipeComponent.OwnedBy = ent;
    }

    private void OnEjectIndex(Entity<TimedCookerComponent> ent, ref TimedCookerEjectIndexedIngredientMessage args)
    {
        if (!GetContents(ent, out var cookingVesselComponent))
            return;
        _sharedContainerSystem.Remove(GetEntity(args.EntityId), cookingVesselComponent.Storage);
        UpdateUserInterfaceState(ent);
    }

    private void OnEjectMsg(Entity<TimedCookerComponent> ent, ref TimedCookerEjectMessage args)
    {
        if (!GetContents(ent, out var cookingVesselComponent))
            return;
        _sharedContainerSystem.EmptyContainer(cookingVesselComponent.Storage);
        _sharedAudioSystem.PlayPredicted(ent.Comp.ClickSound,
            ent.Owner,
            args.Actor,
            AudioParams.Default.WithVolume(-2));
        UpdateUserInterfaceState(ent);
    }


    private void OnContainerContentsUpdate(EntityUid ent,
        TimedCookerComponent timedCookerComponent,
        ContainerModifiedMessage args)
    {
        UpdateUserInterfaceState((ent, timedCookerComponent));
    }

    protected void UpdateUserInterfaceState(Entity<TimedCookerComponent, ActiveTimedCookerComponent?> ent)
    {
        _sharedUserInterface.SetUiState(ent.Owner,
            TimedCookerUiKey.Key,
            new TimedCookerUpdateUserInterfaceState(ent.Comp2?.EndStamp));
    }

    protected bool GetContents(EntityUid ent, [NotNullWhen(true)] out CookingVesselComponent? cookingVesselComponent)
    {
        cookingVesselComponent = null;
        if (!TryComp<CookingVesselComponent>(ent, out var definiteComponent))
            return false;
        cookingVesselComponent = definiteComponent;
        return cookingVesselComponent.Storage.ContainedEntities.Any();
    }
}
