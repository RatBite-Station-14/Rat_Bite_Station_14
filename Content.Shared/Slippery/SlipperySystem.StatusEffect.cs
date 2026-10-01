using Content.Shared._BRatbite.Nutrition;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.StepTrigger.Components;
using Content.Shared.StepTrigger.Systems;

namespace Content.Shared.Slippery;

// Ratbite
public sealed partial class SlipperySystem : EntitySystem
{
    private void SubscribeStatusEffectEvents()
    {
        SubscribeLocalEvent<SlipperyComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<SlipperyComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<SlipperyComponent, StatusEffectRelayedEvent<StepTriggeredOffEvent>>((ent, ref args) => TrySlip(ent.Owner, ent.Comp, args.Args.Tripper));
        SubscribeLocalEvent<SlipperyComponent, StatusEffectRelayedEvent<StepTriggerAttemptEvent>>(OnStepTriggerAttempt);
        SubscribeLocalEvent<SlipperyComponent, EffectDescriptionEvent>(OnGetSlipperyDescription);
    }
    private void OnGetSlipperyDescription(Entity<SlipperyComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-slippery"));
        args.Message.PushNewline();
    }

    private void OnStatusEffectApplied(Entity<SlipperyComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<StepTriggerComponent>(args.Target);
    }

    private void OnStatusEffectRemoved(Entity<SlipperyComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_status.HasEffectComp<SlipperyComponent>(args.Target)) return;
        RemComp<StepTriggerComponent>(args.Target);
    }

    private void OnStepTriggerAttempt(Entity<SlipperyComponent> ent, ref StatusEffectRelayedEvent<StepTriggerAttemptEvent> args)
    {
        if (CompOrNull<StatusEffectComponent>(ent)?.AppliedTo is not { } appliedTo) return;
        args.Args = args.Args with
        {
            Continue = args.Args.Continue | ent.Comp.SlipData.SlipOnStep && CanSlip(appliedTo, args.Args.Tripper)
        };
    }
}
