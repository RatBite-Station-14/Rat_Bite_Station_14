namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class EffectDescriptionStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EffectDescriptionStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnGetDescription(Entity<EffectDescriptionStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString(ent.Comp.Description));
        args.Message.PushNewline();
    }
}
