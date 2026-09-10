using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Construction;
using Content.Server.Temperature.Systems;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;
using Content.Shared.Kitchen;
using Content.Shared.Stacks;
using Content.Shared.Temperature.Components;
using Robust.Server.Containers;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class CookingVesselSystem : SharedCookingVesselSystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _sharedSolutionContainerSystem = default!;
    [Dependency] private readonly RecipeManager _recipeManager = default!;
    [Dependency] private readonly ContainerSystem _containerSystem = default!;
    [Dependency] private readonly SharedStackSystem _stack = default!;
    [Dependency] private readonly TemperatureSystem _temperatureSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingUsedInRecipeComponent, OnConstructionTemperatureEvent>(OnConstructionTemp);
        SubscribeLocalEvent<BeingUsedInRecipeComponent, SolutionRelayEvent<ReactionAttemptEvent>>(OnReactionAttempt);
    }

    private void OnReactionAttempt(Entity<BeingUsedInRecipeComponent> ent,
        ref SolutionRelayEvent<ReactionAttemptEvent> args)
    {
        if (!TryComp<ActiveTimedCookerComponent>(ent.Comp.OwnedBy, out var activeTimedCooker))
            return;

        if (!activeTimedCooker.Recipe.HasValue)
            return;

        foreach (var ingredientsReagentsKey in activeTimedCooker.Recipe.Value.recipe.IngredientsReagents.Keys)
        {
            if (!args.Event.Reaction.Reactants.ContainsKey(ingredientsReagentsKey))
                continue;
            args.Event.Cancelled = true;
            return;
        }
    }

    public void SubtractContents(Entity<CookingVesselComponent> ent, FoodRecipePrototype recipe)
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

    public void ReserveAllIngredients(Entity<CookingVesselComponent> ent)
    {
        foreach (var item in ent.Comp.Storage.ContainedEntities.ToArray())
        {
            var beingUsedInRecipeComponent = EnsureComp<BeingUsedInRecipeComponent>(item);
            beingUsedInRecipeComponent.OwnedBy = ent;
        }
    }

    public void RelinquishAllIngredients(Entity<CookingVesselComponent> ent)
    {
        foreach (var item in ent.Comp.Storage.ContainedEntities.ToArray())
        {
            if (HasComp<BeingUsedInRecipeComponent>(item))
                RemCompDeferred<BeingUsedInRecipeComponent>(item);
        }
    }

    public void AddTemperature(CookingVesselComponent cookingVesselComponent, float frameTime)
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

    public (FoodRecipePrototype recipe, int portions)? GetFirstSatisfiableRecipe(CookingVesselComponent cookingVessel)
    {
        var ingredients = CollectIngredients(cookingVessel.Storage.ContainedEntities);
        return _recipeManager.Recipes
            .Select(x =>
                (recipes: x, portions: CanSatisfyRecipe(x, ingredients.Solids, ingredients.Reagents)))
            .FirstOrNull(p => p.portions > 0);
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
                solids[protoId] += amount;

            if (!_sharedSolutionContainerSystem.TryGetDrainableSolution(item, out var _, out var solution))
                continue;

            foreach (var (reagent, quantity) in solution.Contents)
            {
                if (!reagents.TryAdd(reagent.Prototype, quantity))
                    reagents[reagent.Prototype] += quantity;
            }
        }

        return (solids, reagents);
    }

    private void OnConstructionTemp(Entity<BeingUsedInRecipeComponent> ent, ref OnConstructionTemperatureEvent args)
    {
        args.Result = HandleResult.False;
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
