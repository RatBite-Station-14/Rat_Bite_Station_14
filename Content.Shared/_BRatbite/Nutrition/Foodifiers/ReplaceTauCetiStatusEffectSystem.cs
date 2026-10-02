using Content.Shared._EinsteinEngines.Language;
using Content.Shared._EinsteinEngines.Language.Events;
using Content.Shared._EinsteinEngines.Language.Systems;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public abstract partial class ReplaceTauCetiStatusEffectSystem : EntitySystem
{
    private static ProtoId<LanguagePrototype> TauCeti = "TauCetiBasic";
    [Dependency] private readonly SharedLanguageSystem _languageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReplaceTauCetiStatusEffectComponent, StatusEffectRelayedEvent<DetermineEntityLanguagesEvent>>(OnDetermineLanguage);
        SubscribeLocalEvent<ReplaceTauCetiStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnDetermineLanguage(Entity<ReplaceTauCetiStatusEffectComponent> ent, ref StatusEffectRelayedEvent<DetermineEntityLanguagesEvent> args)
    {
        args.Args.SpokenLanguages.Remove(TauCeti);
        args.Args.SpokenLanguages.Add(ent.Comp.Language);
        if (ent.Comp.RemoveUnderstanding)
            args.Args.UnderstoodLanguages.Remove(TauCeti);
        args.Args.UnderstoodLanguages.Add(ent.Comp.Language);
    }

    private void OnGetEffectDescription(Entity<ReplaceTauCetiStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-replace-tau-ceti", ("language", ent.Comp.Language)));
        args.Message.PushNewline();
    }
}
