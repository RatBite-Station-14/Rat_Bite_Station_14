using System.Linq;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class DamageResistanceStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<DamageResistanceStatusEffectComponent, StatusEffectRelayedEvent<DamageModifyEvent>>(OnDamageModifyEvent);
        SubscribeLocalEvent<DamageResistanceStatusEffectComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnDamageModifyEvent(Entity<DamageResistanceStatusEffectComponent> ent, ref StatusEffectRelayedEvent<DamageModifyEvent> args)
    {
        args.Args.Damage = DamageSpecifier.ApplyModifierSet(args.Args.Damage, DamageSpecifier.PenetrateArmor(ent.Comp.DamageModifier, args.Args.Damage.ArmorPenetration));
    }

    private void OnGetDescription(Entity<DamageResistanceStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        if (ent.Comp.DamageModifier.Coefficients.Any())
        {
            args.Message.AddMarkupOrThrow(
                string.Join(
                    "\n",
                    ent.Comp.DamageModifier.Coefficients.Select(
                        (coefficient) => Loc.GetString("guidebook-description-damage-resistance-coefficient", ("type", coefficient.Key), ("coefficient", MathF.Round(coefficient.Value * 100)))
                    )
                )
            );
            args.Message.PushNewline();
        }

        if (ent.Comp.DamageModifier.FlatReduction.Any())
        {
            args.Message.AddMarkupOrThrow(
                string.Join(
                    "\n",
                    ent.Comp.DamageModifier.FlatReduction.Select(
                        (flatReduction) => Loc.GetString("guidebook-description-damage-resistance-flat-reduction", ("type", flatReduction.Key), ("amount", flatReduction.Value))
                    )
                )
            );
            args.Message.PushNewline();
        }
    }
}
