using System.Linq;
using Content.Shared.Damage;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class TakeDamageOvertimeStatusEffectSystem : OvertimeStatusEffectSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TakeDamageOvertimeStatusEffectComponent, EffectDescriptionEvent>(OnEffectDescription);
    }

    protected override void Tick(TimeSpan elapsedTime)
    {
        var eq = EntityQueryEnumerator<TakeDamageOvertimeStatusEffectComponent, StatusEffectComponent>();
        while (eq.MoveNext(out var uid, out var damageOvertimeComp, out var statusEffect))
        {
            var scale = CompOrNull<Components.StatusEffectScaleComponent>(uid)?.Scale ?? 1f;
            _damageableSystem.TryChangeDamage(statusEffect.AppliedTo, damageOvertimeComp.DamagePerSecond * scale * elapsedTime.TotalSeconds);
        }
    }

    private void OnEffectDescription(Entity<TakeDamageOvertimeStatusEffectComponent> ent, ref EffectDescriptionEvent args)
    {
        var damages = string.Join(", ", ent.Comp.DamagePerSecond.DamageDict.Select((damage) => Loc.GetString("guidebook-description-damage", ("damageType", damage.Key), ("damageAmount", damage.Value))));
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-damage-overtime", ("damages", damages)));
        args.Message.PushNewline();
    }
}
