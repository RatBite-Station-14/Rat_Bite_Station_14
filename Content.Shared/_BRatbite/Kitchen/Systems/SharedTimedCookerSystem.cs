using System.Linq;
using Content.Shared._BRatbite.Kitchen.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Kitchen.Systems;

/// <summary>
/// This handles timed cooking machines. <see cref="TimedCookerComponent"/>
/// </summary>
public abstract class SharedTimedCookerSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _sharedUserInterface = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;
    [Dependency] private readonly SharedCookingVesselSystem _cookingVesselSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<TimedCookerComponent, ComponentInit>(OnInit);
        SubscribeLocalEvent<TimedCookerComponent, EntInsertedIntoContainerMessage>(OnContainerContentsUpdate);
        SubscribeLocalEvent<TimedCookerComponent, EntRemovedFromContainerMessage>(OnContainerContentsUpdate);

        SubscribeLocalEvent<TimedCookerComponent, TimedCookerEjectMessage>(OnEjectMsg);
        SubscribeLocalEvent<TimedCookerComponent, TimedCookerEjectIndexedIngredientMessage>(OnEjectIndex);

        SubscribeLocalEvent<TimedCookerComponent, OpenBoundInterfaceMessage>(OnUIOpen);
        SubscribeLocalEvent<TimedCookerComponent, CloseBoundInterfaceMessage>(OnUIClose);
    }

    private void OnInit(Entity<TimedCookerComponent> ent, ref ComponentInit args)
    {
        if (!TryComp<CookingVesselComponent>(ent, out var cookingVesselComponent))
            return;
        var cookingMultiplier = _prototype.Index(cookingVesselComponent.PreparationMethod).CookTimeMultiplier;
        var modifiedTimeSpans =
            ent.Comp.CookTimePresets.Select((p) => (uint) Math.Ceiling(p * cookingMultiplier)).ToArray();
        ent.Comp.CalculatedPresets = modifiedTimeSpans;
    }

    private void OnContainerContentsUpdate(EntityUid ent,
        TimedCookerComponent timedCookerComponent,
        ContainerModifiedMessage args)
    {
        UpdateUserInterfaceState(ent);
    }

    private void OnEjectMsg(Entity<TimedCookerComponent> ent, ref TimedCookerEjectMessage args)
    {
        _cookingVesselSystem.EjectAllContents(ent.Owner);
        _sharedAudioSystem.PlayPredicted(ent.Comp.ClickSound,
            ent.Owner,
            args.Actor,
            AudioParams.Default.WithVolume(-2));
        UpdateUserInterfaceState(ent);
    }

    private void OnEjectIndex(Entity<TimedCookerComponent> ent, ref TimedCookerEjectIndexedIngredientMessage args)
    {
        _cookingVesselSystem.EjectEntity(ent.Owner, GetEntity(args.EntityId));
        UpdateUserInterfaceState(ent);
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

    protected void UpdateUserInterfaceState(EntityUid ent, TimeSpan? endTime = null)
    {
        _sharedUserInterface.SetUiState(ent,
            TimedCookerUiKey.Key,
            new TimedCookerUpdateUserInterfaceState(endTime));
    }
}
