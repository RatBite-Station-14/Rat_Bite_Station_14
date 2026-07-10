using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Movement.Pulling;

[RegisterComponent]
public sealed partial class WeakPullerComponent : Component
{
    [DataField]
    public ProtoId<ItemSizePrototype> MaxPullableSize = "Small";
}
