using Content.Shared.Humanoid;
using Content.Shared.Inventory;
using Robust.Shared.GameStates;

namespace Content.Shared.Clothing.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class AppendageToggleClothingComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool HideAppendages;

    [DataField(required: true)]
    public HashSet<HumanoidVisualLayers> Layers = new();

    [DataField]
    public SlotFlags Slots = SlotFlags.NONE;

    [DataField]
    public LocId HideMessage = "appendage-toggle-hide";

    [DataField]
    public LocId ShowMessage = "appendage-toggle-show";
}
