using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Temperature.Systems;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Kitchen;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Shared.Temperature.Components;
using Robust.Server.Containers;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class TimedCookerSystem : SharedTimedCookerSystem
{
    [Dependency] private readonly SharedPowerReceiverSystem _sharedPowerReceiverSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sharedSolutionContainerSystem = default!;
    [Dependency] private readonly RecipeManager _recipeManager = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly SharedAudioSystem _sharedAudioSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly SharedAppearanceSystem _sharedAppearanceSystem = default!;

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
                StopCooking((uid, cookingVesselComponent));
                RemCompDeferred<ActiveTimedCookerComponent>(uid);
                if (timedCookerComponent.PlayingStream is null)
                    continue;
                _sharedAudioSystem.Stop(timedCookerComponent.PlayingStream);
                timedCookerComponent.PlayingStream = null;
                _sharedAppearanceSystem.SetData(uid, TimedCookerDataKeys.Active, false);
                continue;
            }

            AddTemperature(cookingVesselComponent, frameTime);
            if (activeTimedCookerComponent.EndStamp.Subtract(_gameTiming.CurTime) > TimeSpan.Zero)
                continue;

            if (activeTimedCookerComponent.Recipe.HasValue)
            {
                for (var i = 0; i < activeTimedCookerComponent.Recipe.Value.portions; i++)
                {
                    SubtractContents((uid, cookingVesselComponent), activeTimedCookerComponent.Recipe.Value.recipe);
                    Spawn(activeTimedCookerComponent.Recipe.Value.recipe.Result, Transform(uid).Coordinates);
                }
            }

            RemCompDeferred<ActiveTimedCookerComponent>(uid);
            _containerSystem.EmptyContainer(cookingVesselComponent.Storage);
            UpdateUserInterfaceState((uid, timedCookerComponent));
            _sharedAudioSystem.PlayPvs(timedCookerComponent.FoodDoneSound, uid, AudioParams.Default);
            StopCooking((uid, cookingVesselComponent));
            if (timedCookerComponent.PlayingStream is null)
                continue;
            _sharedAudioSystem.Stop(timedCookerComponent.PlayingStream);
            timedCookerComponent.PlayingStream = null;
            _sharedAppearanceSystem.SetData(uid, TimedCookerDataKeys.Active, false);
        }
    }

    private void StopCooking(Entity<CookingVesselComponent> ent)
    {
        foreach (var storageContainedEntity in ent.Comp.Storage.ContainedEntities)
        {
            if (HasComp<BeingUsedInRecipeComponent>(storageContainedEntity))
                RemCompDeferred<BeingUsedInRecipeComponent>(storageContainedEntity);
        }
    }

    private void SubtractContents(Entity<CookingVesselComponent> ent, FoodRecipePrototype recipe)
    {
        var reagentBudget = new Dictionary<string, FixedPoint2>(recipe.IngredientsReagents);
        var solidBudget = new Dictionary<string, FixedPoint2>(recipe.IngredientsSolids);

        foreach (var item in ent.Comp.Storage.ContainedEntities.ToArray())
        {
            TakeReagents(item, reagentBudget);
            TakeSolid(ent, item, solidBudget);
        }
    }

    private void TakeReagents(EntityUid item, Dictionary<string, FixedPoint2> budget)
    {
        if (budget.Count == 0)
            return;
        if (!_sharedSolutionContainerSystem.TryGetDrainableSolution(item, out var solutionComp, out var solution))
            return;

        foreach (var (reagent, needed) in budget.ToArray())
        {
            var available = solution.GetTotalPrototypeQuantity(reagent);
            if (available <= 0)
                continue;

            var take = FixedPoint2.Min(available, needed);
            _sharedSolutionContainerSystem.RemoveReagent(solutionComp.Value, reagent, take);

            if (take >= needed)
                budget.Remove(reagent);
            else
                budget[reagent] -= take;
        }
    }

    private void TakeSolid(Entity<CookingVesselComponent> ent, EntityUid item, Dictionary<string, FixedPoint2> budget)
    {
        if (budget.Count == 0)
            return;
        if (GetIngredientId(item, out var amount) is not { } id || !budget.TryGetValue(id, out var needed))
            return;

        if (TryComp<StackComponent>(item, out var stackComponent))
        {
            var take = Math.Min(amount, (int) needed);
            _stack.SetCount((item, stackComponent), stackComponent.Count - take);
            if (stackComponent.Count - take <= 0)
            {
                _containerSystem.Remove(item, ent.Comp.Storage);
                QueueDel(item);
            }

            Consume(budget, id, take);
            return;
        }

        _containerSystem.Remove(item, ent.Comp.Storage);
        QueueDel(item);
        Consume(budget, id, 1);
    }

    private static void Consume(Dictionary<string, FixedPoint2> budget, string id, int amount)
    {
        if ((budget[id] -= amount) <= 0)
            budget.Remove(id);
    }

    private void AddTemperature(CookingVesselComponent cookingVesselComponent, float frameTime)
    {
        foreach (var storageContainedEntity in cookingVesselComponent.Storage.ContainedEntities)
        {
            if (TryComp<TemperatureComponent>(storageContainedEntity, out var temperature))
                _temperatureSystem.ChangeHeat(storageContainedEntity, frameTime, false, temperature);

            if (!TryComp<SolutionContainerManagerComponent>(storageContainedEntity, out var solutionContainerManager))
                continue;
            foreach (var (_, solutionComponent) in _sharedSolutionContainerSystem.EnumerateSolutions((
                         storageContainedEntity, solutionContainerManager)))
            {
                _sharedSolutionContainerSystem.AddThermalEnergy(solutionComponent, frameTime);
            }
        }
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
            beingUsedComponent.OwnedBy = ent;
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

            if (_tag.HasTag(storageContainedEntity, PlasticTag))
            {
                var junk = Spawn(cookingVesselComponent.BadRecipeEntityId, Transform(ent).Coordinates);
                _containerSystem.Insert(junk, cookingVesselComponent.Storage);
                Del(storageContainedEntity);
            }
        }

        var ingredients = CollectIngredients(cookingVesselComponent.Storage.ContainedEntities);
        (FoodRecipePrototype recipe, int portions)? portionedRecipe = _recipeManager.Recipes
            .Select(x =>
                (recipes: x, portions: CanSatisfyRecipe(x, ingredients.Solids, ingredients.Reagents)))
            .FirstOrNull(p => p.portions > 0);

        var cookTimeMultiplier = foodPreparationMethodPrototype.CookTimeMultiplier;
        var calculatedTimeSpans =
            ent.Comp.CookTimePresets.Select(p => (int) Math.Ceiling(p * cookTimeMultiplier)).ToArray();
        var stopCookingAt = args.SelectedTimePreset == 0
            ? _gameTiming.CurTime
            : _gameTiming.CurTime + TimeSpan.FromSeconds(calculatedTimeSpans[args.SelectedTimePreset - 1]);
        var activeComponent = AddComp<ActiveTimedCookerComponent>(ent);
        activeComponent.Recipe = portionedRecipe;
        activeComponent.EndStamp = stopCookingAt;
        _sharedAudioSystem.PlayPvs(ent.Comp.StartCookingSound, ent, AudioParams.Default);
        UpdateUserInterfaceState((ent, ent.Comp, activeComponent));
        if (ent.Comp.LoopingSound is null)
            return;
        ent.Comp.PlayingStream = _sharedAudioSystem
            .PlayPvs(ent.Comp.LoopingSound, ent, AudioParams.Default.WithLoop(true))
            ?.Entity;
        _sharedAppearanceSystem.SetData(ent, TimedCookerDataKeys.Active, true);
    }

    private int CanSatisfyRecipe(FoodRecipePrototype recipe,
        Dictionary<string, int> ingredients,
        Dictionary<string, FixedPoint2> reagents)
    {
        var portions = int.MaxValue;

        foreach (var (id, minQuantity) in recipe.IngredientsSolids)
        {
            if (!ingredients.TryGetValue(id, out var quantity) || quantity < minQuantity)
                return 0;
            portions = Math.Min(portions, quantity / minQuantity.Int());
        }

        foreach (var (id, minQuantity) in recipe.IngredientsReagents)
        {
            if (!reagents.TryGetValue(id, out var quantity) || quantity < minQuantity)
                return 0;
            portions = Math.Min(portions, (quantity / minQuantity).Int());
        }

        return portions == int.MaxValue ? 0 : portions;
    }

    private (Dictionary<string, int> Solids, Dictionary<string, FixedPoint2> Reagents) CollectIngredients(
        IEnumerable<EntityUid> items)
    {
        var solids = new Dictionary<string, int>();
        var reagents = new Dictionary<string, FixedPoint2>();
        foreach (var item in items)
        {
            var protoId = GetIngredientId(item, out var amount);

            if (protoId is null)
                continue;

            if (!solids.TryAdd(protoId, amount))
                solids[protoId] = amount;

            if (!_sharedSolutionContainerSystem.TryGetDrainableSolution(item, out var _, out var solution))
                continue;

            foreach (var (reagent, quantity) in solution.Contents)
            {
                if (!reagents.TryAdd(reagent.Prototype, quantity))
                    reagents[reagent.Prototype] = quantity;
            }
        }

        return (solids, reagents);
    }

    private string? GetIngredientId(EntityUid item, out int amount)
    {
        string? protoId = null;
        amount = 1;

        if (TryComp<StackComponent>(item, out var stackComponent))
        {
            protoId = _prototype.Index(stackComponent.StackTypeId).Spawn;
            amount = stackComponent.Count;
        }
        else
        {
            var metadata = MetaData(item);
            if (metadata.EntityPrototype is not null)
                protoId = metadata.EntityPrototype.ID;
        }

        return protoId;
    }
}
