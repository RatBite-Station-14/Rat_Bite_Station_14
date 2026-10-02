using Content.Server._EinsteinEngines.Language;
using Content.Shared._BRatbite.Nutrition.Foodifiers;
using Content.Shared.StatusEffectNew;

namespace Content.Server._BRatbite.Nutrition.Foodifiers;

public sealed partial class ServerReplaceTauCetiStatusEffectSystem : ReplaceTauCetiStatusEffectSystem
{
    [Dependency] private readonly LanguageSystem _languageSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ReplaceTauCetiStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<ReplaceTauCetiStatusEffectComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
    }

    private void OnEffectApplied(Entity<ReplaceTauCetiStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        _languageSystem.UpdateEntityLanguages(args.Target);
    }

    private void OnEffectRemoved(Entity<ReplaceTauCetiStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        _languageSystem.UpdateEntityLanguages(args.Target);
    }
}
