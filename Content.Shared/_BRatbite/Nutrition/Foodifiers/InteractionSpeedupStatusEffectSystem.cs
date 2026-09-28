using Content.Shared._BRatbite.DoAfter;
using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Body.Part;
using Content.Shared.Chemistry.Events;
using Content.Shared.DoAfter;
using Content.Shared.Medical;
using Content.Shared.StatusEffectNew;
using static Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class InteractionSpeedupStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeInteractionDoAfterEvent<ToolInteractionSpeedupStatusEffectComponent, ToolDoAfterEvent>("guidebook-description-speedup-tools");
        SubscribeInteractionDoAfterEvent<SurgeryInteractionSpeedupStatusEffectComponent, SurgeryDoAfterEvent>("guidebook-description-speedup-surgery");
        SubscribeInteractionDoAfterEvent<InjectionInteractionSpeedupStatusEffectComponent, InjectorDoAfterEvent>("guidebook-description-speedup-injection");
        SubscribeInteractionDoAfterEvent<HealingInteractionSpeedupStatusEffectComponent, HealingDoAfterEvent>("guidebook-description-speedup-healing");
        SubscribeInteractedDoAfterEvent<SurgeryInteractedSpeedupStatusEffectComponent, SurgeryDoAfterEvent>("guidebook-description-speedup-surgery");
        SubscribeInteractedDoAfterEvent<InjectionInteractedSpeedupStatusEffectComponent, InjectorDoAfterEvent>("guidebook-description-speedup-injection");
        SubscribeInteractedDoAfterEvent<HealingInteractedSpeedupStatusEffectComponent, HealingDoAfterEvent>("guidebook-description-speedup-healing");

        SubscribeLocalEvent<BodyPartComponent, GetDoAfterTargetMultiplierEvent>(OnDoAfterTargetMultiplierEvent);
    }

    private void SubscribeInteractionDoAfterEvent<T, U>(LocId speedupLocId)
        where T: InteractionSpeedupStatusEffectComponent
        where U: DoAfterEvent
    {
        SubscribeLocalEvent<T, StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent>>((ent, ref args) => OnDoAfterMultiplier<U>((ent.Owner, ent.Comp), ref args));
        SubscribeLocalEvent<T, EffectDescriptionEvent>((ent, ref args) => OnInteractionGetEffectDescription((ent.Owner, ent.Comp), speedupLocId, ref args));
    }

    private void SubscribeInteractedDoAfterEvent<T, U>(LocId speedupLocId)
        where T: InteractedSpeedupStatusEffectComponent
        where U: DoAfterEvent
    {
        SubscribeLocalEvent<T, StatusEffectRelayedEvent<GetDoAfterTargetMultiplierEvent>>((ent, ref args) => OnDoAfterMultiplier<U>((ent.Owner, ent.Comp), ref args));
        SubscribeLocalEvent<T, EffectDescriptionEvent>((ent, ref args) => OnInteractedGetEffectDescription((ent.Owner, ent.Comp), speedupLocId, ref args));
    }

    private void OnDoAfterMultiplier<T>(Entity<InteractionSpeedupStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
        where T: DoAfterEvent
    {
        if (args.Args.Event is not T) return;

        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args.Multiplier *= ent.Comp.Multiplier * scale;
    }
    private void OnDoAfterMultiplier<T>(Entity<InteractedSpeedupStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetDoAfterTargetMultiplierEvent> args)
    {
        if (args.Args.Event is not T) return;
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args = args.Args with { Multiplier = args.Args.Multiplier * ent.Comp.Multiplier * scale };
    }

    private void OnInteractionGetEffectDescription(Entity<InteractionSpeedupStatusEffectComponent> ent, LocId speedupLocId, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-speedup-effect" , ("multiplier", MathF.Round((1 / ent.Comp.Multiplier) * 100)), ("effect", Loc.GetString(speedupLocId))));
        args.Message.PushNewline();
    }

    private void OnInteractedGetEffectDescription(Entity<InteractedSpeedupStatusEffectComponent> ent, LocId speedupLocId, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-speedup-interacted-effect" , ("multiplier", MathF.Round((1 / ent.Comp.Multiplier) * 100)), ("effect", Loc.GetString(speedupLocId))));
        args.Message.PushNewline();
    }

    // We need this special case for body parts to make surgery speedup work
    private void OnDoAfterTargetMultiplierEvent(Entity<BodyPartComponent> ent, ref GetDoAfterTargetMultiplierEvent args)
    {
        if (ent.Comp.Body is not { } body) return;
        RaiseLocalEvent(body, ref args);
    }
}
