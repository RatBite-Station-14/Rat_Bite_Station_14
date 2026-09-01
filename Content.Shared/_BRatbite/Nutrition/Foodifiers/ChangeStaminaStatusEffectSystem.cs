using Content.Shared.Damage.Components;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChangeStaminaStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeStaminaStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusApplied);
        SubscribeLocalEvent<ChangeStaminaStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusRemoved);
        SubscribeLocalEvent<ChangeStaminaStatusEffectComponent, StatusEffectScaleEvent>(OnStatusScale);
    }

    private void OnStatusApplied(Entity<ChangeStaminaStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        if (!TryComp<StaminaComponent>(args.Target, out var stamina)) return;
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        stamina.CritThreshold += ent.Comp.AddedStamina * scale;
        Dirty(args.Target, stamina);
    }

    private void OnStatusRemoved(Entity<ChangeStaminaStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (!TryComp<StaminaComponent>(args.Target, out var stamina)) return;
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        stamina.CritThreshold -= ent.Comp.AddedStamina * scale;
        Dirty(args.Target, stamina);
    }

    private void OnStatusScale(Entity<ChangeStaminaStatusEffectComponent> ent, ref StatusEffectScaleEvent args)
    {
        if (!TryComp<StaminaComponent>(args.Target, out var stamina)) return;
        stamina.CritThreshold += ent.Comp.AddedStamina * (args.NewScale - args.OldScale);
        Dirty(args.Target, stamina);
    }
}
