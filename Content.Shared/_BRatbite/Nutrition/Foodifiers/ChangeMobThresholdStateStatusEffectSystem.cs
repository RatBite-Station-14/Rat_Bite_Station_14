using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChangeMobThresholdStateStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly MobThresholdSystem _mobThresholdSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeMobThresholdStateStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ChangeMobThresholdStateStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<ChangeMobThresholdStateStatusEffectComponent, StatusEffectScaleEvent>(OnStatusEffectScale);
        SubscribeLocalEvent<ChangeMobThresholdStateStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnStatusEffectApplied(Entity<ChangeMobThresholdStateStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        var threshold = _mobThresholdSystem.GetThresholdForState(args.Target, ent.Comp.State);
        _mobThresholdSystem.SetMobStateThreshold(args.Target, threshold + ent.Comp.Amount * scale, ent.Comp.State);
    }

    private void OnStatusEffectRemoved(Entity<ChangeMobThresholdStateStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        var threshold = _mobThresholdSystem.GetThresholdForState(args.Target, ent.Comp.State);
        _mobThresholdSystem.SetMobStateThreshold(args.Target, threshold - ent.Comp.Amount * scale, ent.Comp.State);
    }

    private void OnStatusEffectScale(Entity<ChangeMobThresholdStateStatusEffectComponent> ent, ref StatusEffectScaleEvent args)
    {
        var threshold = _mobThresholdSystem.GetThresholdForState(args.Target, ent.Comp.State);
        _mobThresholdSystem.SetMobStateThreshold(args.Target, threshold + ent.Comp.Amount * (args.NewScale - args.OldScale), ent.Comp.State);
    }

    private void OnGetDescription(Entity<ChangeMobThresholdStateStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-mob-state-threshold", ("state", ent.Comp.State), ("amount", ent.Comp.Amount)));
        args.Message.PushNewline();
    }
}
