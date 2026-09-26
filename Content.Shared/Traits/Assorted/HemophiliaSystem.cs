using Content.Shared._BRatbite.Nutrition;
using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Body.Events;
using Content.Shared.StatusEffectNew;

namespace Content.Shared.Traits.Assorted;

public sealed class HemophiliaSystem : EntitySystem
{
    public override void Initialize()
    {
        SubscribeLocalEvent<HemophiliaStatusEffectComponent, StatusEffectRelayedEvent<BleedModifierEvent>>(OnBleedModifier);
        SubscribeLocalEvent<HemophiliaStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription); // Ratbite
    }

    private void OnBleedModifier(Entity<HemophiliaStatusEffectComponent> ent, ref StatusEffectRelayedEvent<BleedModifierEvent> args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f; // Ratbite: added scale
        var ev = args.Args;
        ev.BleedReductionAmount *= ent.Comp.BleedReductionMultiplier * scale;
        ev.BleedAmount *= ent.Comp.BleedAmountMultiplier * scale;
        args.Args = ev;
    }

    // Ratbite start
    private void OnGetEffectDescription(Entity<HemophiliaStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        if (ent.Comp.BleedReductionMultiplier != 1f)
        {
            args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-hemophilia-bleed-reduction", ("amount", MathF.Round(ent.Comp.BleedReductionMultiplier * 100))));
            args.Message.PushNewline();
        }

        if (ent.Comp.BleedAmountMultiplier != 1f)
        {
            args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-hemophilia-bleed-multiplier", ("amount", MathF.Round(ent.Comp.BleedAmountMultiplier * 100))));
            args.Message.PushNewline();
        }
    }
    // Ratbite end
}
