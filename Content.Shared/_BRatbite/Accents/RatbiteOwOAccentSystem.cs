using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Speech.Components;

namespace Content.Shared._BRatbite.Accents;

public sealed partial class RatbiteOwOAccentSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<OwOAccentComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnGetEffectDescription(Entity<OwOAccentComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-owo-accent"));
        args.Message.PushNewline();
    }
}
