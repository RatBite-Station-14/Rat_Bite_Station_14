using Content.Server.Construction;
using Content.Shared._BRatbite.Kitchen.Components;
using Content.Shared._BRatbite.Kitchen.Systems;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reaction;

namespace Content.Server._BRatbite.Machines;

/// <inheritdoc/>
public sealed class CookingVesselSystem : SharedCookingVesselSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BeingUsedInRecipeComponent, OnConstructionTemperatureEvent>(OnConstructionTemp);
        SubscribeLocalEvent<BeingUsedInRecipeComponent, SolutionRelayEvent<ReactionAttemptEvent>>(OnReactionAttempt);
    }

    private void OnReactionAttempt(Entity<BeingUsedInRecipeComponent> ent, ref SolutionRelayEvent<ReactionAttemptEvent> args)
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


    private void OnConstructionTemp(Entity<BeingUsedInRecipeComponent> ent, ref OnConstructionTemperatureEvent args)
    {
        args.Result = HandleResult.False;
    }
}
