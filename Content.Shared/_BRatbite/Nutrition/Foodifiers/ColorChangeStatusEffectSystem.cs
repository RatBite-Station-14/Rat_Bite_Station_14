using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public abstract partial class ColorChangeStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearanceSystem = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ColorChangeStatusEffectComponent, StatusEffectAppliedEvent>(OnStatusEffectApplied);
        SubscribeLocalEvent<ColorChangeStatusEffectComponent, StatusEffectRemovedEvent>(OnStatusEffectRemoved);
        SubscribeLocalEvent<ColorChangeStatusEffectComponent, EffectDescriptionEvent>(OnGetEffectDescription);
    }

    private void OnStatusEffectApplied(Entity<ColorChangeStatusEffectComponent> ent, ref StatusEffectAppliedEvent args)
    {
        EnsureComp<ColorChangeComponent>(args.Target);
        _appearanceSystem.SetData(args.Target, ColorStatusEffectVisuals.Color, ent.Comp.Color);
    }

    private void OnStatusEffectRemoved(Entity<ColorChangeStatusEffectComponent> ent, ref StatusEffectRemovedEvent args)
    {
        if (_statusEffectsSystem.HasEffectComp<ColorChangeStatusEffectComponent>(args.Target)) return;
        _appearanceSystem.SetData(args.Target, ColorStatusEffectVisuals.Color, Color.White);
    }

    private void OnGetEffectDescription(Entity<ColorChangeStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-color-change", ("color", ent.Comp.Color)));
        args.Message.PushNewline();
    }
}
