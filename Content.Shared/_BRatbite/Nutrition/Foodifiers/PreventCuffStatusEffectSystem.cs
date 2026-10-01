using Content.Shared.Cuffs.Components;
using Content.Shared.IdentityManagement;
using Content.Shared.Popups;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class PreventCuffStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PreventCuffStatusEffectComponent, StatusEffectRelayedEvent<CuffAttemptEvent>>(OnCuffAttempt);
        SubscribeLocalEvent<PreventCuffStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnCuffAttempt(Entity<PreventCuffStatusEffectComponent> ent, ref StatusEffectRelayedEvent<CuffAttemptEvent> args)
    {
        if (CompOrNull<StatusEffectComponent>(ent)?.AppliedTo is not { } appliedTo) return;
        args.Args = args.Args with { Cancelled = true };
        if (!_timing.InPrediction) _popup.PopupEntity(Loc.GetString("status-effect-cuff-failed-attempt", ("name", Identity.Name(appliedTo, EntityManager))), appliedTo);
        RemComp<PreventCuffStatusEffectComponent>(ent);
    }

    private void OnGetDescription(Entity<PreventCuffStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-prevent-cuff"));
        args.Message.PushNewline();
    }
}
