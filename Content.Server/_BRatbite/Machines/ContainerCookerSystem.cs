using System.Linq;
using Content.Server.Chemistry.Components;
using Content.Server.Popups;
using Content.Server.Power.EntitySystems;
using Content.Server.Temperature.Systems;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Atmos;
using Content.Shared.Placeable;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Machines;

/// <summary>
/// Handles containers that cook stuff.
/// <seealso cref="ContainerCookerComponent" />
/// </summary>
public sealed class ContainerCookerSystem : SharedContainerCookerSystem
{
    [Dependency] private readonly SharedAudioSystem _audioSystem = default!;
    [Dependency] private readonly CookingVesselSystem _cookingVesselSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly PopupSystem _popupSystem = default!;
    [Dependency] private readonly PowerReceiverSystem _powerReceiverSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;

    /// <inheritdoc />
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ActiveContainerCookerComponent, EntInsertedIntoContainerMessage>(OnActiveCookerInsert);
        SubscribeLocalEvent<ActiveContainerCookerComponent, EntRemovedFromContainerMessage>(OnActiveCookerRemove);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeSolutionHeaters =
            EntityQueryEnumerator<SolutionHeaterComponent, ItemPlacerComponent, ActiveSolutionHeaterComponent>();
        while (activeSolutionHeaters.MoveNext(out var uid, out _, out var itemPlacerComponent, out _))
        {
            foreach (var placedEntity in itemPlacerComponent.PlacedEntities.Where(HasComp<ContainerCookerComponent>))
            {
                StartCooking(placedEntity, uid);
            }
        }

        var activeEntityHeaters = EntityQueryEnumerator<EntityHeaterComponent, ItemPlacerComponent>();
        while (activeEntityHeaters.MoveNext(out var uid, out var entityHeaterComponent, out var itemPlacerComponent))
        {
            if (!_powerReceiverSystem.IsPowered(uid) || entityHeaterComponent.Setting == EntityHeaterSetting.Off)
                continue;

            foreach (var placedEntity in itemPlacerComponent.PlacedEntities.Where(HasComp<ContainerCookerComponent>))
            {
                StartCooking(placedEntity, uid);
            }
        }

        var activeCookers = EntityQueryEnumerator<ContainerCookerComponent, ActiveContainerCookerComponent>();
        while (activeCookers.MoveNext(out var uid, out _, out var activeContainerCookerComponent))
        {
            if (!_powerReceiverSystem.IsPowered(activeContainerCookerComponent.HeatSource) ||
                !TryComp<ItemPlacerComponent>(activeContainerCookerComponent.HeatSource, out var itemPlacerComponent) ||
                !itemPlacerComponent.PlacedEntities.ToArray().Contains(uid) ||
                TryComp<EntityHeaterComponent>(activeContainerCookerComponent.HeatSource, out var activeHeater) &&
                activeHeater.Setting == EntityHeaterSetting.Off)
                StopCooking(uid);

            UpdateActiveHeater(uid, frameTime);
        }
    }

    private void OnActiveCookerRemove(Entity<ActiveContainerCookerComponent> ent,
        ref EntRemovedFromContainerMessage args)
    {
        if (HasComp<BeingUsedInRecipeComponent>(args.Entity))
            RemCompDeferred<BeingUsedInRecipeComponent>(args.Entity);
    }

    private void OnActiveCookerInsert(Entity<ActiveContainerCookerComponent> ent,
        ref EntInsertedIntoContainerMessage args)
    {
        var beingUsedInRecipeComponent = AddComp<BeingUsedInRecipeComponent>(args.Entity);
        beingUsedInRecipeComponent.OwnedBy = ent;
    }

    private void UpdateActiveHeater(EntityUid entity, float frameTime)
    {
        if (!TryComp<CookingVesselComponent>(entity, out var cookingVessel) ||
            !TryComp<ContainerCookerComponent>(entity, out var cookerComponent))
            return;
        _cookingVesselSystem.AddTemperature(cookingVessel, frameTime);
        _temperatureSystem.ChangeHeat(entity, frameTime);
        if (cookerComponent.EndCookingTime > _gameTiming.CurTime)
            return;
        if (cookerComponent.Recipe.HasValue)
        {
            for (var i = 0; i < cookerComponent.Recipe.Value.Item2; i++)
            {
                _cookingVesselSystem.SubtractContents((entity, cookingVessel), cookerComponent.Recipe.Value.Item1);
                Spawn(cookerComponent.Recipe.Value.Item1.Result, Transform(entity).Coordinates);
            }
        }

        StopCooking(entity);
    }

    private void StopCooking(EntityUid entity)
    {
        if (!TryComp<CookingVesselComponent>(entity, out var cookingVessel) ||
            !TryComp<ContainerCookerComponent>(entity, out var cookerComponent))
            return;
        _cookingVesselSystem.RelinquishAllIngredients((entity, cookingVessel));
        _temperatureSystem.ForceChangeTemperature(entity, Atmospherics.T20C); // reset heat tint
        RemCompDeferred<ActiveContainerCookerComponent>(entity);
        cookerComponent.EndCookingTime = null;
        cookerComponent.Recipe = null;
    }

    private void StartCooking(EntityUid entity, EntityUid heatSource)
    {
        if (!TryComp<CookingVesselComponent>(entity, out var cookingVessel) ||
            !TryComp<ContainerCookerComponent>(entity, out var cookerComponent) ||
            HasComp<ActiveContainerCookerComponent>(entity))
            return;
        _cookingVesselSystem.ReserveAllIngredients((entity, cookingVessel));
        var recipe = _cookingVesselSystem.GetFirstSatisfiableRecipe(cookingVessel);
        if (!recipe.HasValue)
            return;
        var cookTimeMultiplier = _prototypeManager.Index(cookingVessel.PreparationMethod).CookTimeMultiplier;
        cookerComponent.Recipe = recipe;
        cookerComponent.EndCookingTime = _gameTiming.CurTime +
                                         TimeSpan.FromSeconds(recipe.Value.recipe.CookTime * cookTimeMultiplier *
                                                              recipe.Value.portions);
        var activeComponent = AddComp<ActiveContainerCookerComponent>(entity);
        activeComponent.HeatSource = heatSource;
        _audioSystem.PlayPvs(cookerComponent.StartCookingSound, entity, AudioParams.Default);
        _popupSystem.PopupEntity(Loc.GetString("cooking-vessel-component-cooking-start"), entity);
    }
}
