using Content.Shared.Damage;
using Content.Shared.Hands.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared._BRatbite.PermaBrig;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Weapons.Melee;


public sealed class SaberDamageSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _handsSystem = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<CaptainSaberWeaknessComponent, DamageModifyEvent>(OnDamageModifyCaptain);
        SubscribeLocalEvent<PrisonerComponent, DamageModifyEvent>(OnDamageModifyHos);
    }

    private void OnDamageModifyCaptain(Entity<CaptainSaberWeaknessComponent> ent, ref DamageModifyEvent args)
    {
        var origin = args.Origin;
        if (origin != null && TryComp<HandsComponent>(origin, out var handsComponent))
        {
            if (_handsSystem.TryGetActiveItem(new(origin.Value, handsComponent), out var item) && HasComp<CaptainSaberComponent>(item))
            {
                args.Damage *= ent.Comp.DamageMultiplier;
            }
        }
    }

    private void OnDamageModifyHos(Entity<PrisonerComponent> ent, ref DamageModifyEvent args)
    {
        var origin = args.Origin;
        if (origin != null && TryComp<HandsComponent>(origin, out var handsComponent))
        {
            if (_handsSystem.TryGetActiveItem(new(origin.Value, handsComponent), out var item) && TryComp<HosSaberComponent>(item, out var hosSaber))
            {
                var remainingSentence = (ent.Comp.PermaBrigSentenceExpireTime - _gameTiming.CurTime).TotalMinutes;
                args.Damage += (hosSaber.DamageMultiplierPerMinute * (float) Math.Max(remainingSentence, 0) * args.Damage);
            }
        }
    }
}

