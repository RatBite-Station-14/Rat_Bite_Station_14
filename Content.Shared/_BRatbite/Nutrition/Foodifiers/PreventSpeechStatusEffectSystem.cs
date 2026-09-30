using Content.Shared._EinsteinEngines.Language.Systems;
using Content.Shared.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

// We don't use the MutedComponent becuase it would clash with other
// systems, and porting the entire thing to the new status effect
// would take too much time
public sealed partial class PreventSpeechStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedLanguageSystem _languageSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PreventSpeechStatusEffectComponent, StatusEffectRelayedEvent<SpeakAttemptEvent>>(OnSpeakAttemptEvent);
        SubscribeLocalEvent<PreventSpeechStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnSpeakAttemptEvent(Entity<PreventSpeechStatusEffectComponent> ent, ref StatusEffectRelayedEvent<SpeakAttemptEvent> args)
    {
        var language = _languageSystem.GetLanguage(ent.Owner);
        if (!language.SpeechOverride.RequireSpeech) return;
        args.Args.Cancel();
    }

    private void OnGetEffectDescription(Entity<PreventSpeechStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-prevent-speech"));
        args.Message.PushNewline();
    }
}
