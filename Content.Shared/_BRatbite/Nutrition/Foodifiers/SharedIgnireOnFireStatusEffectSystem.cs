namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public abstract class SharedIgniteOnFireStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<IgniteOnFireStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescriptionEvent);
    }

    private void OnGetEffectDescriptionEvent(Entity<IgniteOnFireStatusEffectComponent> entity,
        ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-ignite"));
        args.Message.PushNewline();
    }
}
