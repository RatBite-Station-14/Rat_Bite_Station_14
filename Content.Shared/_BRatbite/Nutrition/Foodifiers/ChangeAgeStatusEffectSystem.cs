using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Humanoid;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class ChangeAgeStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChangeAgeStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ChangeAgeStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<ChangeAgeStatusEffectComponent, StatusEffectScaleEvent>(OnStatusEffectScale);
        SubscribeLocalEvent<ChangeAgeStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnStatusEffectApplied(Entity<ChangeAgeStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoidAppearance)) return;
        humanoidAppearance.Age += (int) (ent.Comp.Amount * scale);
    }

    private void OnStatusEffectRemoved(Entity<ChangeAgeStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoidAppearance)) return;
        humanoidAppearance.Age -= (int) (ent.Comp.Amount * scale);
    }

    private void OnStatusEffectScale(Entity<ChangeAgeStatusEffectComponent> ent, ref StatusEffectScaleEvent args)
    {
        if (!TryComp<HumanoidAppearanceComponent>(args.Target, out var humanoidAppearance)) return;
        humanoidAppearance.Age -= (int) (ent.Comp.Amount * args.OldScale);
        humanoidAppearance.Age += (int) (ent.Comp.Amount * args.NewScale);
    }

    private void OnGetDescription(Entity<ChangeAgeStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString(ent.Comp.Amount >= 0 ? "guidebook-description-change-age-positive" : "guidebook-description-change-age-negative", ("amount", MathF.Abs(ent.Comp.Amount))));
        args.Message.PushNewline();
    }
}
