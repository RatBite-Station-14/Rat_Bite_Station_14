using Content.Shared.Chemistry;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Chemistry;

// We need to share these fields so we can predict the UI
// temperatures
[NetworkedComponent]
public abstract partial class SharedChemMasterComponent : Component
{
    [DataField]
    public float HeatPerSecond = 120f; // A bit slower than the hotplate
    // list of reagents contained in the chem master
    [ViewVariables]
    public Dictionary<ReagentId, ReagentQuantityTemperature> Reagents = new ();

    [ViewVariables]
    public float TargetHeat = 293f;

    [ViewVariables]
    public ReagentId? SelectedReagentToHeat;
}
