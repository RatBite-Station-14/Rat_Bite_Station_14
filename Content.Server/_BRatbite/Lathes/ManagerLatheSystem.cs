using Content.Shared.Lathe;
using Content.Shared._BRatbite.Lathe;
using Content.Server.Wires;
using Content.Shared.Wires;
using Robust.Shared.Prototypes;
using Content.Shared.Research.Components;

namespace Content.Server._BRatbite.Lathes;

public sealed class ManagerLatheSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    public override void Initialize()
    {
        SubscribeLocalEvent<ManagerLatheRecipesComponent, LatheGetRecipesEvent>(OnGetRecipes);
    }

    private void OnGetRecipes(Entity<ManagerLatheRecipesComponent> ent, ref LatheGetRecipesEvent args)
    {
        var comp = ent.Comp;
        if (!comp.Cut) return;

        foreach (var id in comp.ManagerStaticPacks)
        {
            var pack = _proto.Index(id);
            foreach (var recipe in pack.Recipes)
            {
                args.Recipes.Add(recipe);
            }
        }

        if (!TryComp<TechnologyDatabaseComponent>(ent.Owner, out var database)) return;

        foreach (var id in comp.ManagerDynamicPacks)
        {
            var pack = _proto.Index(id);
            foreach (var recipe in pack.Recipes)
            {
                if (args.GetUnavailable || database.UnlockedRecipes.Contains(recipe))
                {
                    args.Recipes.Add(recipe);
                }
            }
        }
    }

    public bool ManagerWireCut(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        comp.Cut = true;
        return true;
    }

    public bool ManagerWireMend(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        comp.Cut = false;
        return true;
    }

    public void ManagerWirePulse(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        // Pulsing does nothing
        return;
    }
}
