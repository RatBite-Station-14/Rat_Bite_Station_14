using Robust.Shared.Random;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class GrantRandomComponentSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GrantRandomComponentComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<GrantRandomComponentComponent> ent, ref ComponentStartup args)
    {
        EntityManager.AddComponent(ent, _random.Pick(ent.Comp.Components).Value, overwrite: true);
        RemCompDeferred<GrantRandomComponentComponent>(ent);
    }
}
