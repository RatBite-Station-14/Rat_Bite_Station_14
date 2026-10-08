using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class GrantRandomComponentComponent : Component
{
    [DataField(required: true)]
    public ComponentRegistry Components = new ();
}
