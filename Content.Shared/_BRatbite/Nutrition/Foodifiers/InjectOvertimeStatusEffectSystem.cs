using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.StatusEffectNew.Components;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public sealed partial class InjectOvertimeStatusEffectSystem : OvertimeStatusEffectSystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    public override void Initialize()
    {
        base.Initialize();
    }

    protected override void Tick(TimeSpan elapsedTime)
    {
        var eq = EntityQueryEnumerator<InjectOvertimeStatusEffectComponent, StatusEffectComponent>();
        while (eq.MoveNext(out var uid, out var injectOvertimeComp, out var statusEffect))
        {
            var scale = CompOrNull<StatusEffectScaleComponent>(uid)?.Scale ?? 1f;
            if (!TryComp<SolutionComponent>(uid, out var solutionComp)) continue;
            if (statusEffect.AppliedTo is not { } appliedTo) continue;
            if (!_solutionContainers.TryGetInjectableSolution(appliedTo, out var targetSoln, out var targetSolution))
                continue;
            var transferAmount = FixedPoint2.Min(injectOvertimeComp.InjectAmountPerSecond * scale * elapsedTime.TotalSeconds, targetSolution.AvailableVolume);
            if (transferAmount <= 0) continue;
            var solution = solutionComp.Solution; // No need to clone, we aren't removing it from anywhere
            solution.ScaleTo(transferAmount);
            if (!targetSolution.CanAddSolution(solution)) continue;
            _solutionContainers.TryAddSolution(targetSoln.Value, solution);
        }
    }
}
