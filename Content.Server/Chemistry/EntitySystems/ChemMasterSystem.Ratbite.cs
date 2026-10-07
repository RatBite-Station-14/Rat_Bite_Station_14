using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Server.Chemistry.Components;
using Content.Server.Power.Components;
using Content.Shared._BRatbite.Chemistry;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server.Chemistry.EntitySystems;

// Ratbite chem master refactor: Don't use solutions because they don't allow us to make temperatures per reagent.
// Store a dictionary of reagents and their temperature and quantity instead
public sealed partial class ChemMasterSystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Update(float _)
    {
        base.Update(_);
        LoopChemMasters<ChemMasterComponent, ApcPowerReceiverComponent>();
    }

    private void SubscribeRatbiteEvents()
    {
        SubscribeLocalEvent<ChemMasterComponent, ChemMasterSelectReagentToHeatMessage>(OnReagentToHeatSelected);
        SubscribeLocalEvent<ChemMasterComponent, ChemMasterSetHeatMessage>(OnSetHeat);
    }


    private void AddReagent(Entity<ChemMasterComponent> ent, ReagentQuantityTemperature reagent)
    {
        if (!ent.Comp.Reagents.TryGetValue(reagent.Reagent, out var r))
        {
            ent.Comp.Reagents.Add(reagent.Reagent, reagent);
            return;
        }
        ent.Comp.Reagents[reagent.Reagent] = new
            (reagent.Reagent, r.Quantity + reagent.Quantity, (r.Temperature * r.Quantity.Float() + reagent.Temperature * reagent.Quantity.Float()) / (r.Quantity.Float() + reagent.Quantity.Float()));
    }

    private ReagentQuantityTemperature RemoveReagent(Entity<ChemMasterComponent> ent, ReagentQuantity reagentToTake)
    {
        if (!ent.Comp.Reagents.TryGetValue(reagentToTake.Reagent, out var reagent)) return new (reagentToTake.Reagent, 0, 0);
        var amountToTake = FixedPoint2.Min(reagentToTake.Quantity, reagent.Quantity);
        if (amountToTake == reagent.Quantity)
            ent.Comp.Reagents.Remove(reagent.Reagent);
        else
        {
            reagent.Quantity -= amountToTake;
            ent.Comp.Reagents[reagent.Reagent] = reagent;
        }
        return new(reagent.Reagent, amountToTake, reagent.Temperature);

    }

    // Remove amount from the buffer and put it in a solution
    // Popup the user in case of error
    private bool RemoveAmount(Entity<ChemMasterComponent> ent, FixedPoint2 amount, EntityUid? user, [NotNullWhen(true)] out Solution? solution)
    {
        solution = null;
        if (ent.Comp.Reagents.Count == 0)
        {
            if (user is { } uid)
                _popupSystem.PopupCursor(Loc.GetString("chem-master-window-buffer-empty-text"), uid);
            return false;
        }
        var volume = ent.Comp.Reagents.Values.Sum(r => r.Quantity.Float());
        if (amount > volume)
        {
            if (user is { } uid)
                _popupSystem.PopupCursor(Loc.GetString("chem-master-window-buffer-low-text"), uid);
            return false;
        }
        solution = new Solution(ent.Comp.Reagents.Count);
        foreach (var (_, reagent) in ent.Comp.Reagents)
        {
            var volumeToRemove = amount * reagent.Quantity / volume;
            var proto = _proto.Index<ReagentPrototype>(reagent.Reagent.Prototype);
            solution.AddReagent(proto, volumeToRemove, reagent.Temperature, _proto);
            if (reagent.Quantity == volumeToRemove)
                ent.Comp.Reagents.Remove(reagent.Reagent);
            else
                ent.Comp.Reagents[reagent.Reagent] = new(reagent.Reagent, reagent.Quantity - volumeToRemove, reagent.Temperature);
        }
        return true;
    }

    private void TryReachTargetHeat(Entity<ChemMasterComponent> ent, float deltaTime)
    {
        if (ent.Comp.SelectedReagentToHeat is not { } reagent || !ent.Comp.Reagents.TryGetValue(reagent, out var reagentData)) return;

        var heatCapacity = _proto.Index<ReagentPrototype>(reagent.Prototype).SpecificHeat * reagentData.Quantity.Float();
        var maxTempDelta = ent.Comp.HeatPerSecond * deltaTime / heatCapacity;
        var deltaTemp = float.Clamp(ent.Comp.TargetHeat - reagentData.Temperature, -maxTempDelta, maxTempDelta);
        ent.Comp.Reagents[reagent] = new(reagent, reagentData.Quantity, reagentData.Temperature + deltaTemp);
    }

    private void OnReagentToHeatSelected(Entity<ChemMasterComponent> ent, ref ChemMasterSelectReagentToHeatMessage args)
    {
        ent.Comp.SelectedReagentToHeat = args.Reagent;
        UpdateUiState(ent);
        ClickSound(ent);
    }

    private void OnSetHeat(Entity<ChemMasterComponent> ent, ref ChemMasterSetHeatMessage args)
    {
        if (!float.IsFinite(args.Temperature) || args.Temperature < 0) return;
        ent.Comp.TargetHeat = args.Temperature;
        UpdateUiState(ent);
        ClickSound(ent);
    }
}
