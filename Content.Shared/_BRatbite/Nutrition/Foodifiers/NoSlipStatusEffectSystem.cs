using Content.Shared.Slippery;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class NoSlipStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<NoSlipStatusEffectComponent, StatusEffectRelayedEvent<SlipAttemptEvent>>(OnSlipAttempt);
        SubscribeLocalEvent<NoSlipStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescriptionEvent);
        SubscribeLocalEvent<NoSlipStatusEffectComponent, StatusEffectRelayedEvent<GetSlowedOverSlipperyModifierEvent>>(OnGetSlowedOverSlipperyModifierEvent);
    }

    private void OnSlipAttempt(Entity<NoSlipStatusEffectComponent> ent, ref StatusEffectRelayedEvent<SlipAttemptEvent> args)
    {
        args.Args.NoSlip = true;
        args.Args.SlowOverSlippery = true;
    }

    private void OnGetEffectDescriptionEvent(Entity<NoSlipStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-no-slip"));
        args.Message.PushNewline();
    }

    private void OnGetSlowedOverSlipperyModifierEvent(Entity<NoSlipStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetSlowedOverSlipperyModifierEvent> args)
    {
        args.Args = args.Args with { SlowdownModifier = ent.Comp.SlowDownOverSlipperyMultiplier };
    }
}
