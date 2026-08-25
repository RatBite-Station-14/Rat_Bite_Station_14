using Content.Shared.Damage;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class TakeDamageOvertimeStatusEffectSystem : OvertimeStatusEffectSystem
{
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;

    protected override void Tick(TimeSpan elapsedTime)
    {
        var eq = EntityQueryEnumerator<TakeDamageOvertimeStatusEffectComponent, StatusEffectComponent>();
        while (eq.MoveNext(out var uid, out var damageOvertimeComp, out var statusEffect))
        {
            var scale = CompOrNull<StatusEffectScaleComponent>(uid)?.Scale ?? 1f;
            _damageableSystem.TryChangeDamage(statusEffect.AppliedTo, damageOvertimeComp.DamagePerSecond * scale * elapsedTime.TotalSeconds);
        }
    }
}
