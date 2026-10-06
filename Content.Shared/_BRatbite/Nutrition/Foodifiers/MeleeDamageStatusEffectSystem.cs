using System.Linq;
using Content.Shared.StatusEffectNew;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class MeleeDamageStatusEffectSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MeleeDamageStatusEffectComponent, StatusEffectRelayedEvent<GetMeleeDamageEvent>>(OnGetMeleeDamage);
        SubscribeLocalEvent<MeleeDamageStatusEffectComponent, EffectDescriptionEvent>(OnEffectDescription);
    }

    private void OnGetMeleeDamage(Entity<MeleeDamageStatusEffectComponent> ent, ref StatusEffectRelayedEvent<GetMeleeDamageEvent> args)
    {
        var ev = args.Args;
        if (ent.Comp.OnlyPunches && ev.User != ev.Weapon) return;
        ev.Damage *= ent.Comp.Multiplier;
        if (ent.Comp.ExtraDamage != null)
            ev.Damage += ent.Comp.ExtraDamage;
        args.Args = ev;
    }

    private void OnEffectDescription(Entity<MeleeDamageStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        if (ent.Comp.Multiplier is > 0.99f and < 1.01f)
        {
            args.Message.AddMarkupOrThrow(Loc.GetString(ent.Comp.OnlyPunches ? "guidebook-description-melee-buff-punch" : "guidebook-description-melee-buff", ("multiplier", MathF.Round(ent.Comp.Multiplier * 100))));
            args.Message.PushNewline();
        }

        if (ent.Comp.ExtraDamage != null)
        {
            args.Message.AddMarkupOrThrow(Loc.GetString(ent.Comp.OnlyPunches ? "guidebook-description-melee-buff-extra-punch" : "guidebook-description-melee-buff-extra", ("newDamage", string.Join(", ", ent.Comp.ExtraDamage.DamageDict.Select((d) => Loc.GetString("guidebook-description-damage", ("damageAmount", d.Value), ("damageType", d.Key)))))));
            args.Message.PushNewline();
        }
    }
}
