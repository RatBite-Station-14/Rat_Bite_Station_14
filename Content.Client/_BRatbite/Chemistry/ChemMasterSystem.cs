using Content.Client.Power.Components;
using Content.Shared._BRatbite.Chemistry;

namespace Content.Client._BRatbite.Chemistry;

public sealed partial class ChemMasterSystem : SharedChemMasterSystem
{
    public override void Update(float _)
    {
        base.Update(_);
        if (_timing.IsFirstTimePredicted)
            LoopChemMasters<ChemMasterComponent, ApcPowerReceiverComponent>();
    }
}
