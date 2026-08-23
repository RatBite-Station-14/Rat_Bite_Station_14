using Content.Goobstation.Shared.InternalResources.Data;
using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Goobstation.Shared.Changeling.Components;

/// <summary>
/// Marks a changeling that has evolved Void Adaption.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class VoidAdaptionComponent : Component
{
    public ProtoId<AlertPrototype> Alert = "VoidAdaption";

    [DataField]
    public ProtoId<InternalResourcesPrototype> ResourceType = "ChangelingChemicals";

    [DataField, AutoNetworkedField]
    public bool FirePopupSent;
    public LocId FirePopup = "changeling-voidadapt-onfire";

    public bool AdaptingLowPressure;
    public LocId EnterLowPressurePopup = "changeling-voidadapt-lowpressure-start";
    public LocId LeaveLowPressurePopup = "changeling-voidadapt-lowpressure-end";

    public bool AdaptingLowTemp;
    public LocId EnterLowTempPopup = "changeling-voidadapt-lowtemperature-start";
    public LocId LeaveLowTempPopup = "changeling-voidadapt-lowtemperature-end";

    [DataField, AutoNetworkedField]
    public float ChemModifierValue = 1f;
}
