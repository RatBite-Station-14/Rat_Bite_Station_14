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
        SubscribeDoAfterEvent<ToolInteractionSpeedupStatusEffectComponent, ToolDoAfterEvent>();
        SubscribeDoAfterEvent<SurgeryInteractionSpeedupStatusEffectComponent, SurgeryDoAfterEvent>();
        SubscribeDoAfterEvent<InjectionInteractionSpeedupStatusEffectComponent, InjectorDoAfterEvent>();
    }

    private void SubscribeDoAfterEvent<T, U>()
        where T: InteractionSpeedupStatusEffectComponent
        where U: DoAfterEvent
    {
        SubscribeLocalEvent<T, StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent>>((ent, ref args) => OnDoAfterMultiplier<U>((ent.Owner, ent.Comp), ref args));
    }

    private void OnDoAfterMultiplier<T>(Entity<InteractionSpeedupStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetDoAfterDelayMultiplierEvent> args)
        where T: DoAfterEvent
    {
        if (args.Args.Event is not T) return;

        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args.Multiplier *= ent.Comp.Multiplier * scale;
    }
}
