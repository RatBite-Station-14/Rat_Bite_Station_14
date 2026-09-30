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
        args.Args = ev;
    }

    private void OnEffectDescription(Entity<MeleeDamageStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString(ent.Comp.OnlyPunches ? "guidebook-description-melee-buff-punch" : "guidebook-description-melee-buff", ("multiplier", MathF.Round(ent.Comp.Multiplier * 100))));
        args.Message.PushNewline();
    }
}
