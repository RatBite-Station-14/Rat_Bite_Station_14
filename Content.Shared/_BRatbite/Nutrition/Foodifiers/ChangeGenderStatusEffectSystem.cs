using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Enums;
using Robust.Shared.GameObjects.Components.Localization;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChangeGenderStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly GrammarSystem _grammarSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangeGenderStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<ChangeGenderStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
        SubscribeLocalEvent<ChangeGenderStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnStatusApplied(Entity<ChangeGenderStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<GrammarComponent>(args.Target, out var grammar))
            return;
        _grammarSystem.SetGender((args.Target, grammar), ent.Comp.NewGender);
    }

    private void OnStatusRemoved(Entity<ChangeGenderStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TryComp<GrammarComponent>(args.Target, out var grammar))
            return;
        if (_statusEffectsSystem.HasEffectComp<ChangeGenderStatusEffectComponent>(args.Target)) return;
        var oldGender = CompOrNull<HumanoidAppearanceComponent>(args.Target)?.Gender ?? Gender.Neuter;
        _grammarSystem.SetGender((args.Target, grammar), oldGender);
    }

    private void OnGetDescription(Entity<ChangeGenderStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        var pronouns = ent.Comp.NewGender switch
        {
            Gender.Neuter => "guidebook-pronouns-neuter",
            Gender.Epicene => "guidebook-pronouns-epicene",
            Gender.Female => "guidebook-pronouns-female",
            Gender.Male => "guidebook-pronouns-male",
            _ => ""
        };
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-change-gender", ("pronouns", Loc.GetString(pronouns))));
        args.Message.PushNewline();
    }
}
