using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Construction;
using Content.Server.Temperature.Systems;
using Content.Shared._BRatbite.Kitchen;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Kitchen;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Stacks;
using Content.Shared.Tag;
using Content.Shared.Temperature.Components;
using Robust.Server.Containers;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class CookingVesselSystem : SharedCookingVesselSystem
{
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly RecipeManager _recipeManager = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sharedSolutionContainerSystem = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _sharedPowerReceiverSystem = default!;
    [Dependency] private readonly TagSystem _tag = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    private static readonly ProtoId<TagPrototype> MetalTag = "Metal";
    private static readonly ProtoId<TagPrototype> PlasticTag = "Plastic";

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingUsedInRecipeComponent, OnConstructionTemperatureEvent>(OnConstructionTemp);
        SubscribeLocalEvent<BeingUsedInRecipeComponent, SolutionRelayEvent<ReactionAttemptEvent>>(OnReactionAttempt);

        SubscribeLocalEvent<CookingVesselComponent, EntInsertedIntoContainerMessage>(OnItemInsert);
        SubscribeLocalEvent<CookingVesselComponent, EntRemovedFromContainerMessage>(OnItemRemove);
    }

    private void OnItemRemove(Entity<CookingVesselComponent> ent, ref EntRemovedFromContainerMessage args)
    {
        if (HasComp<BeingUsedInRecipeComponent>(args.Entity))
            RemCompDeferred<BeingUsedInRecipeComponent>(args.Entity);
    }

    private void OnItemInsert(Entity<CookingVesselComponent> ent, ref EntInsertedIntoContainerMessage args)
    {
        if (!HasComp<ActiveCookingVesselComponent>(ent))
            return;
        var beingUsedComp = AddComp<BeingUsedInRecipeComponent>(args.Entity);
        beingUsedComp.OwnedBy = ent;
    }

    private void OnConstructionTemp(Entity<BeingUsedInRecipeComponent> ent, ref OnConstructionTemperatureEvent args)
    {
        args.Result = HandleResult.False;
    }

    private void OnReactionAttempt(Entity<BeingUsedInRecipeComponent> ent,
        ref SolutionRelayEvent<ReactionAttemptEvent> args)
    {
        if (!TryComp<ActiveCookingVesselComponent>(ent.Comp.OwnedBy, out var activeVessel))
            return;

        if (!activeVessel.Recipe.HasValue)
            return;

        foreach (var ingredientsReagentsKey in activeVessel.Recipe.Value.recipe.IngredientsReagents.Keys)
        {
            if (!args.Event.Reaction.Reactants.ContainsKey(ingredientsReagentsKey))
                continue;
            args.Event.Cancelled = true;
            return;
        }
    }

    private void SubtractContents(Entity<CookingVesselComponent> ent, FoodRecipePrototype recipe)
    {
        var reagentBudget = new Dictionary<string, FixedPoint2>(recipe.IngredientsReagents);
        var solidBudget = new Dictionary<string, FixedPoint2>(recipe.IngredientsSolids);

        TakeIngredientReagents(ent, reagentBudget); // prioritize solutions on the vessel first. such as pots.
        foreach (var item in ent.Comp.Storage.ContainedEntities.ToArray())
        {
            TakeIngredientReagents(item, reagentBudget);
            TakeSolid(ent, item, solidBudget);
        }
    }

    private void TakeIngredientReagents(EntityUid item, Dictionary<string, FixedPoint2> budget)
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

    private (FoodRecipePrototype recipe, int portions)? GetFirstSatisfiableRecipe(EntityUid ent, CookingVesselComponent cookingVessel)
    {
        var ingredients = CollectIngredients(ent, cookingVessel.Storage.ContainedEntities);
        return _recipeManager.Recipes
            .Select(x =>
                (recipes: x, portions: CanSatisfyRecipe(x, ingredients.Solids, ingredients.Reagents)))
            .FirstOrNull(p => p.portions > 0);
    }

    /// <summary>
    /// Initiates the process for the vessel to start cooking.
    /// </summary>
    /// <param name="entity">the cooking vessel</param>
    /// <param name="startedBySession">player that started it, used for events items may have when being microwaved.</param>
    /// <param name="updateInterface">invoked to update the vessel's UI.</param>
    /// <param name="withTime">specify only if it should cook for an exact time instead of the bare minimum to complete a recipe.
    ///                        used by timed cookers since the user selects the time and can be wrong.</param>
    /// <returns>a timespan, when it will stop cooking, if it successfully started. null otherwise.</returns>
    public TimeSpan? StartCooking(EntityUid entity,
        Action updateInterface,
        EntityUid? startedBySession = null,
        uint? withTime = null)
    {
        if (!GetContents(entity, out var cookingVesselComponent) ||
            HasComp<ActiveCookingVesselComponent>(entity) ||
            cookingVesselComponent.RequiresPower && !_sharedPowerReceiverSystem.IsPowered(entity))
            return null;

        var preparationMethod = _prototype.Index(cookingVesselComponent.PreparationMethod);
        foreach (var ingredientUid in cookingVesselComponent.Storage.ContainedEntities.ToArray())
        {
            var beingUsedComp = AddComp<BeingUsedInRecipeComponent>(ingredientUid);
            beingUsedComp.OwnedBy = entity;
            if (preparationMethod.ID != "Microwaving")
                continue;
            var specialEvent = new BeingMicrowavedEvent(ingredientUid, startedBySession);
            RaiseLocalEvent(ingredientUid, specialEvent);

            if (specialEvent.Handled)
            {
                updateInterface.Invoke();
                return null;
            }

            if (_tag.HasTag(ingredientUid, MetalTag))
            {
                AddComp<MalfunctioningMicrowaveComponent>(entity);
            }

            if (!_tag.HasTag(ingredientUid, PlasticTag))
                continue;
            Spawn(cookingVesselComponent.BadRecipeEntityId, Transform(entity).Coordinates);
            Del(ingredientUid);
        }

        var portionedRecipe = GetFirstSatisfiableRecipe(entity, cookingVesselComponent);
        var minTimeForResult = portionedRecipe.HasValue
            ? portionedRecipe.Value.recipe.CookTime * preparationMethod.CookTimeMultiplier *
              portionedRecipe.Value.portions
            : 0;
        var stopCookingAt = _gameTiming.CurTime + TimeSpan.FromSeconds(withTime ?? minTimeForResult);
        var activeComp = AddComp<ActiveCookingVesselComponent>(entity);
        activeComp.StopCookingAt = stopCookingAt;
        activeComp.Recipe = portionedRecipe;
        updateInterface.Invoke();
        return stopCookingAt;
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

    private (Dictionary<string, int> Solids, Dictionary<string, FixedPoint2> Reagents) CollectIngredients( EntityUid ent,
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
                solids[protoId] += amount;

            if (!_sharedSolutionContainerSystem.TryGetDrainableSolution(item, out var _, out var solution))
                continue;

            foreach (var (reagent, quantity) in solution.Contents)
            {
                if (!reagents.TryAdd(reagent.Prototype, quantity))
                    reagents[reagent.Prototype] += quantity;
            }
        }


        if (!_sharedSolutionContainerSystem.TryGetDrainableSolution(ent, out var _, out var entSolution))
            return (solids, reagents);


        foreach (var (reagent, quantity) in entSolution.Contents)
        {
            if (!reagents.TryAdd(reagent.Prototype, quantity))
                reagents[reagent.Prototype] += quantity;
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

    public void StopCooking(EntityUid entity)
    {
        if (!HasComp<ActiveCookingVesselComponent>(entity) ||
            !TryComp<CookingVesselComponent>(entity, out var cookingVesselComponent))
            return;
        foreach (var item in cookingVesselComponent.Storage.ContainedEntities.ToArray())
        {
            if (HasComp<BeingUsedInRecipeComponent>(item))
                RemCompDeferred<BeingUsedInRecipeComponent>(item);
        }
        if (HasComp<MalfunctioningMicrowaveComponent>(entity))
            RemCompDeferred<MalfunctioningMicrowaveComponent>(entity);
        RemComp<ActiveCookingVesselComponent>(entity);
        var ev = new CookingVesselFinishedCooking();
        RaiseLocalEvent(entity, ref ev);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var activeCookers = EntityQueryEnumerator<CookingVesselComponent, ActiveCookingVesselComponent>();
        while (activeCookers.MoveNext(out var uid, out var cookingVesselComp,  out var activeComp))
        {
            if (cookingVesselComp.RequiresPower && !_sharedPowerReceiverSystem.IsPowered(uid))
            {
                StopCooking(uid);
            }
            AddTemperature(cookingVesselComp, frameTime);
            if (activeComp.StopCookingAt > _gameTiming.CurTime)
                continue;
            if (activeComp.Recipe.HasValue)
            {
                for (var i = 0; i < activeComp.Recipe.Value.portions; i++)
                {
                    SubtractContents((uid, cookingVesselComp), activeComp.Recipe.Value.recipe);
                    Spawn(activeComp.Recipe.Value.recipe.Result, Transform(uid).Coordinates);
                }
            }
            StopCooking(uid);
            _containerSystem.EmptyContainer(cookingVesselComp.Storage);
        }
    }
}
