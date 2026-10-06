using Content.Server.Atmos.EntitySystems;
using Content.Shared._BRatbite.Atmos;
using Content.Shared._BRatbite.Nutrition.Foodifiers;
using Content.Shared.Atmos.Components;
using Content.Shared.StatusEffectNew;

namespace Content.Server._BRatbite.Nutrition.Foodifiers;

public sealed class IgniteOnFireStatusEffectSystem : SharedIgniteOnFireStatusEffectSystem
{
    [Dependency] private readonly FlammableSystem _flammable = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IgniteOnFireStatusEffectComponent, StatusEffectAppliedEvent>(OnEffectApplied);
        SubscribeLocalEvent<IgniteOnFireStatusEffectComponent, StatusEffectRemovedEvent>(OnEffectRemoved);
        SubscribeLocalEvent<IgniteOnFireStatusEffectComponent, StatusEffectRelayedEvent<ExtinguishAttemptEvent>>(OnExtinguishAttempt);
    }

    private void OnEffectApplied(Entity<IgniteOnFireStatusEffectComponent> entity, ref StatusEffectAppliedEvent args)
    {
        if(!TryComp<FlammableComponent>(args.Target, out var flammableComponent))
            return;
        flammableComponent.MinimumFireStacks += entity.Comp.GuaranteedFireStacks;
        _flammable.AdjustFireStacks(args.Target, entity.Comp.FireStacksToAdd, null, true);
    }

    private void OnEffectRemoved(Entity<IgniteOnFireStatusEffectComponent> entity, ref StatusEffectRemovedEvent args)
    {
        if(!TryComp<FlammableComponent>(args.Target, out var flammableComponent))
            return;
        flammableComponent.MinimumFireStacks -= entity.Comp.GuaranteedFireStacks;
    }

    private void OnExtinguishAttempt(Entity<IgniteOnFireStatusEffectComponent> ent, ref StatusEffectRelayedEvent<ExtinguishAttemptEvent> args)
    {
        if (ent.Comp.GuaranteedFireStacks > 0f)
            args.Args = args.Args with { Cancelled = true };
    }
}
