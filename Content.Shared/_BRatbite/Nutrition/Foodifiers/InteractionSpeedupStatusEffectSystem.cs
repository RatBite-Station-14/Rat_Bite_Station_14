using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared._Shitmed.Medical.Surgery;
using Content.Shared.Chemistry.Events;
using Content.Shared.DoAfter;
using Content.Shared.StatusEffectNew;
using static Content.Shared.Tools.Systems.SharedToolSystem;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class InteractionSpeedupStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeDoAfterEvent<ToolInteractionSpeedupStatusEffectComponent, ToolDoAfterEvent>("guidebook-description-speedup-tools");
        SubscribeDoAfterEvent<SurgeryInteractionSpeedupStatusEffectComponent, SurgeryDoAfterEvent>("guidebook-description-speedup-surgery");
        SubscribeDoAfterEvent<InjectionInteractionSpeedupStatusEffectComponent, InjectorDoAfterEvent>("guidebook-description-speedup-injection");
    }

    private void SubscribeDoAfterEvent<T, U>(LocId speedupLocId)
        where T: InteractionSpeedupStatusEffectComponent
        where U: DoAfterEvent
    {
        SubscribeLocalEvent<T, StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent>>((ent, ref args) => OnDoAfterMultiplier<U>((ent.Owner, ent.Comp), ref args));
        SubscribeLocalEvent<T, EffectDescriptionEvent>((ent, ref args) => OnGetEffectDescription((ent.Owner, ent.Comp), speedupLocId, ref args));
    }

    private void OnDoAfterMultiplier<T>(Entity<InteractionSpeedupStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
        where T: DoAfterEvent
    {
        if (args.Args.Event is not T) return;

        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args.Multiplier *= ent.Comp.Multiplier * scale;
    }

    private void OnGetEffectDescription(Entity<InteractionSpeedupStatusEffectComponent> ent, LocId speedupLocId, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-speedup-effect" , ("multiplier", MathF.Round((1 / ent.Comp.Multiplier) * 100)), ("effect", Loc.GetString(speedupLocId))));
        args.Message.PushNewline();
    }
}
