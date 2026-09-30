using Content.Shared._BRatbite.Nutrition;

namespace Content.Shared._BRatbite.Accents;

public sealed partial class RatbiteFrenchAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<FrenchAccentComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnGetEffectDescription(Entity<FrenchAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-french-accent"));
        args.Message.PushNewline();
    }

}
