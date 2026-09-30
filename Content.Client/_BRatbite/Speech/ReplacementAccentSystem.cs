using Content.Shared._BRatbite.Nutrition;

namespace Content.Client._BRatbite.Speech;

public sealed partial class ReplacementAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReplacementAccentComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnGetDescription(Entity<ReplacementAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-replacement-accent-description", ("name", ent.Comp.Accent)));
        args.Message.PushNewline();
    }
}
