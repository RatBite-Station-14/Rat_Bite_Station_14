using Robust.Shared.Prototypes;
namespace Content.Shared._BRatbite.Interact;

[RegisterComponent]
public sealed partial class AddComponentsOnInteractComponent : Component
{
    [DataField]
    public ComponentRegistry Components;

    [DataField]
    public string? PopupText;
}
