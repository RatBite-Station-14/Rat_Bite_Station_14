using System.Linq;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Kitchen;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Tag;
using Robust.Server.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class TimedCookerSystem : SharedTimedCookerSystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _sharedPowerReceiverSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;
    [Dependency] private readonly CookingVesselSystem _cookingVesselSystem = default!;

    private static readonly ProtoId<TagPrototype> MetalTag = "Metal";
    private static readonly ProtoId<TagPrototype> PlasticTag = "Plastic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<TimedCookerComponent, TimedCookerStartCookingMessage>(OnStartCookingMsg);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeCookers =
            EntityQueryEnumerator<TimedCookerComponent, ActiveTimedCookerComponent, CookingVesselComponent>();
        while (activeCookers.MoveNext(out var uid,
                   out var timedCookerComponent,
                   out var activeTimedCookerComponent,
                   out var cookingVesselComponent))
        {
            if (!_sharedPowerReceiverSystem.IsPowered(uid))
            {
                StopCooking((uid, timedCookerComponent, cookingVesselComponent));
                continue;
            }

            _cookingVesselSystem.AddTemperature(cookingVesselComponent, frameTime);
            if (activeTimedCookerComponent.EndStamp > _gameTiming.CurTime)
                continue;

            if (activeTimedCookerComponent.Recipe.HasValue)
            {
                for (var i = 0; i < activeTimedCookerComponent.Recipe.Value.portions; i++)
                {
                    _cookingVesselSystem.SubtractContents((uid, cookingVesselComponent),
                        activeTimedCookerComponent.Recipe.Value.recipe);
                    Spawn(activeTimedCookerComponent.Recipe.Value.recipe.Result, Transform(uid).Coordinates);
                }
            }

            StopCooking((uid, timedCookerComponent, cookingVesselComponent));
            _containerSystem.EmptyContainer(cookingVesselComponent.Storage);
            _sharedAudioSystem.PlayPvs(timedCookerComponent.FoodDoneSound, uid, AudioParams.Default);
        }
    }

    private void StopCooking(Entity<TimedCookerComponent, CookingVesselComponent> ent)
    {
        RemCompDeferred<ActiveTimedCookerComponent>(ent);
        if (ent.Comp1.PlayingStream is not null)
            ent.Comp1.PlayingStream = _sharedAudioSystem.Stop(ent.Comp1.PlayingStream);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.Active, false);
        ent.Comp2.Cooking = false;
        _cookingVesselSystem.RelinquishAllIngredients((ent.Owner, ent.Comp2));
        UpdateUserInterfaceState((ent.Owner, ent.Comp1));
    }


    private void OnStartCookingMsg(Entity<TimedCookerComponent> ent, ref TimedCookerStartCookingMessage args)
    {
        if (!GetContents(ent, out var cookingVesselComponent) || !_sharedPowerReceiverSystem.IsPowered(ent.Owner) ||
            HasComp<ActiveTimedCookerComponent>(ent))
            return;

        if (args.SelectedTimePreset > ent.Comp.CookTimePresets.Length + 1)
        {
            Log.Warning(
                $"Error when starting timed cooker ${ent.Owner}. Received invalid time preset from ${args.Actor}.");
            return;
        }


        var foodPreparationMethodPrototype = _prototype.Index(cookingVesselComponent.PreparationMethod);
        foreach (var storageContainedEntity in cookingVesselComponent.Storage.ContainedEntities)
        {
            var beingUsedComponent = AddComp<BeingUsedInRecipeComponent>(storageContainedEntity);
            beingUsedComponent.OwnedBy =
                ent; // Though there is a call for this in CookingVesselSystem, might as well do it in this loop instead.
            if (foodPreparationMethodPrototype.ID != "Microwaving")
                continue;
            var specialEvent = new BeingMicrowavedEvent(ent, args.Actor);
            RaiseLocalEvent(storageContainedEntity, specialEvent);

            if (specialEvent.Handled)
            {
                UpdateUserInterfaceState(ent);
                return;
            }

            if (_tag.HasTag(storageContainedEntity, MetalTag))
            {
                AddComp<MalfunctioningMicrowaveComponent>(ent);
            }

            if (!_tag.HasTag(storageContainedEntity, PlasticTag))
                continue;

            var junk = Spawn(cookingVesselComponent.BadRecipeEntityId, Transform(ent).Coordinates);
            _containerSystem.Insert(junk, cookingVesselComponent.Storage);
            Del(storageContainedEntity);
        }

        var portionedRecipe = _cookingVesselSystem.GetFirstSatisfiableRecipe(cookingVesselComponent);
        var cookTimeMultiplier = foodPreparationMethodPrototype.CookTimeMultiplier;
        var calculatedTimeSpans =
            ent.Comp.CookTimePresets.Select(p => (int) Math.Ceiling(p * cookTimeMultiplier)).ToArray();

        // time preset 0 is instant, [1..N] is on the calculated time spans [0..N-1]
        var cookSeconds = args.SelectedTimePreset == 0
            ? 0
            : calculatedTimeSpans[args.SelectedTimePreset - 1];

        // cook time must be an exact match for the portions they make. forces players to learn recipes
        var timeToCook = portionedRecipe.HasValue
            ? portionedRecipe.Value.recipe.CookTime * cookTimeMultiplier * portionedRecipe.Value.portions
            : 0;

        var stopCookingAt = _gameTiming.CurTime + TimeSpan.FromSeconds(cookSeconds);

        var activeComponent = AddComp<ActiveTimedCookerComponent>(ent);
        activeComponent.Recipe = portionedRecipe.HasValue && (int) Math.Ceiling(timeToCook) == cookSeconds ? portionedRecipe : null;
        activeComponent.EndStamp = stopCookingAt;

        _sharedAudioSystem.PlayPvs(ent.Comp.StartCookingSound, ent, AudioParams.Default);
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.Active, true);
        cookingVesselComponent.Cooking = true;
        UpdateUserInterfaceState((ent, ent.Comp, activeComponent));

        if (ent.Comp.LoopingSound is null)
            return;

        ent.Comp.PlayingStream = _sharedAudioSystem
            .PlayPvs(ent.Comp.LoopingSound, ent, AudioParams.Default.WithLoop(true))
            ?.Entity;
    }
}
