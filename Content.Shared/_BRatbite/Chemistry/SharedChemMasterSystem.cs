using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Power.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Chemistry;

public abstract partial class SharedChemMasterSystem : EntitySystem
{
    [Dependency] protected readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float _)
    {

    }

    protected void LoopChemMasters<T, U>()
        where T : SharedChemMasterComponent
        where U : SharedApcPowerReceiverComponent
    {
        var eq = EntityQueryEnumerator<T, U>();
        while (eq.MoveNext(out var uid, out var chemMaster, out var apcPower))
        {
            if (!apcPower.Powered) continue;
            TryReachTargetHeat((uid, chemMaster), (float) _timing.FrameTime.TotalSeconds);
        }
    }

    private void TryReachTargetHeat(Entity<SharedChemMasterComponent> ent, float deltaTime)
    {
        if (ent.Comp.SelectedReagentToHeat is not { } reagent || !ent.Comp.Reagents.TryGetValue(reagent, out var reagentData)) return;
        var heatCapacity = _proto.Index<ReagentPrototype>(reagent.Prototype).SpecificHeat * reagentData.Quantity.Float();
        var maxTempDelta = ent.Comp.HeatPerSecond * deltaTime / heatCapacity;
        var deltaTemp = float.Clamp(ent.Comp.TargetHeat - reagentData.Temperature, -maxTempDelta, maxTempDelta);
        ent.Comp.Reagents[reagent] = new(reagent, reagentData.Quantity, reagentData.Temperature + deltaTemp);
    }
}
