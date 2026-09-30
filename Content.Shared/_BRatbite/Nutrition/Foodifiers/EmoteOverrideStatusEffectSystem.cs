using Content.Goobstation.Common.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class EmoteOverrideStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmoteOverrideStatusEffectComponent, StatusEffectRelayedEvent<GetEmoteSoundsEvent>>(OnEmoteOverride);
        SubscribeLocalEvent<EmoteOverrideStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnEmoteOverride(Entity<EmoteOverrideStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetEmoteSoundsEvent> args)
    {
        args.Args = args.Args with { Handled = true, EmoteSoundProtoId = ent.Comp.Sounds };
    }

    private void OnGetEffectDescription(Entity<EmoteOverrideStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-emote-override", ("emotes", ent.Comp.Sounds)));
        args.Message.PushNewline();
    }
}
