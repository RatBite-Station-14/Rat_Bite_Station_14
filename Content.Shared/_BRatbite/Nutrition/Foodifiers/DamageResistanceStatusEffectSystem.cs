using System.Linq;
using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class DamageResistanceStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageResistanceStatusEffectComponent, StatusEffectRelayedEvent<DamageModifyEvent>>(OnDamageModifyEvent);
        SubscribeLocalEvent<DamageResistanceStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);

        SubscribeLocalEvent<WideSwingResistanceStatusEffectComponent, StatusEffectRelayedEvent<AttackedEvent>>(OnAttackedEvent);
        SubscribeLocalEvent<WideSwingResistanceStatusEffectComponent, EffectDescriptionEvent>(OnGetWideSwingDescription);
    }

    private void OnDamageModifyEvent(Entity<DamageResistanceStatusEffectComponent> ent, ref StatusEffectRelayedEvent<DamageModifyEvent> args)
    {
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, DamageSpecifier.PenetrateArmor(GetScaledDamageModifier(ent.Comp.DamageModifier, scale), args.Args.Damage.ArmorPenetration));
    }

    private DamageModifierSet GetScaledDamageModifier(DamageModifierSet set, float scale)
    {
        if (scale == 1f) return set;

        return new ()
        {
            Coefficients = set.Coefficients.Select(c => (c.Key, c.Value * scale)).ToDictionary(),
            FlatReduction = set.FlatReduction.Select(c => (c.Key, c.Value * scale)).ToDictionary(),
        };
    }

    private void OnGetDescription(Entity<DamageResistanceStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        GetDescriptionForDamageResisitance(ent.Comp.DamageModifier, false, ref args);
    }

    private void GetDescriptionForDamageResisitance(DamageModifierSet damageModifier, bool wideSwing, ref EffectDescriptionEvent args)
    {
        if (damageModifier.Coefficients.Any())
        {
            args.Message.AddMarkupOrThrow(
                string.Join(
                    "\n",
                    damageModifier.Coefficients.Select(
                        (coefficient) => Loc.GetString("guidebook-description-damage-resistance-coefficient", ("type", coefficient.Key), ("coefficient", MathF.Round(coefficient.Value * 100)), ("wideSwing", wideSwing))
                    )
                )
            );
            args.Message.PushNewline();
        }

        if (damageModifier.FlatReduction.Any())
        {
            args.Message.AddMarkupOrThrow(
                string.Join(
                    "\n",
                    damageModifier.FlatReduction.Select(
                        (flatReduction) => Loc.GetString("guidebook-description-damage-resistance-flat-reduction", ("type", flatReduction.Key), ("amount", flatReduction.Value), ("wideSwing", wideSwing))
                    )
                )
            );
            args.Message.PushNewline();
        }
    }

    private void OnAttackedEvent(Entity<WideSwingResistanceStatusEffectComponent> ent, ref StatusEffectRelayedEvent<AttackedEvent> args)
    {
        if (!args.Args.WideSwing) return;
        var scale = CompOrNull<StatusEffectScaleComponent>(ent)?.Scale ?? 1f;        
        args.Args.ModifiersList.Add(GetScaledDamageModifier(ent.Comp.DamageModifier, scale));
    }

    private void OnGetWideSwingDescription(Entity<WideSwingResistanceStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        GetDescriptionForDamageResisitance(ent.Comp.DamageModifier, true, ref args);
    }
}
