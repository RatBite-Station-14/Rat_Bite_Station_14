using Content.Server._BRatbite.Speech.Components;
using Content.Server.Speech.Components;
using Content.Shared._BRatbite.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Drunk;
using Content.Shared.StatusEffect;

namespace Content.Server._BRatbite.Chemistry;

public sealed class CharcoalKetamineCleanupSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;
    [Dependency] private readonly SharedDrunkSystem _drunk = default!;

    private const float CharcoalClearThreshold = 10f;

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var ketamineSedationQuery = EntityQueryEnumerator<KetamineSedationComponent>();
        while (ketamineSedationQuery.MoveNext(out var uid, out _))
        {
            TryClearEffects(uid);
        }

        var ketamineSlurQuery = EntityQueryEnumerator<KetamineSlurredAccentComponent>();
        while (ketamineSlurQuery.MoveNext(out var uid, out _))
        {
            TryClearEffects(uid);
        }

        var slurQuery = EntityQueryEnumerator<SlurredAccentComponent>();
        while (slurQuery.MoveNext(out var uid, out _))
        {
            TryClearEffects(uid);
        }
    }

    private void TryClearEffects(EntityUid uid)
    {
        if (!ShouldClearWithCharcoal(uid))
            return;

        if (HasComp<KetamineSedationComponent>(uid))
            RemCompDeferred<KetamineSedationComponent>(uid);

        if (HasComp<KetamineSlurredAccentComponent>(uid))
            RemCompDeferred<KetamineSlurredAccentComponent>(uid);

        _drunk.TryRemoveDrunkenness(uid);

        if (HasComp<SlurredAccentComponent>(uid))
            RemCompDeferred<SlurredAccentComponent>(uid);
    }

    private bool ShouldClearWithCharcoal(EntityUid uid)
    {
        return _solutions.GetTotalPrototypeQuantity(uid, "Charcoal").Float() >= CharcoalClearThreshold;
    }
}
