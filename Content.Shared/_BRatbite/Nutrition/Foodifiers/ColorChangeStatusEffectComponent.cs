using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

[RegisterComponent]
public sealed partial class ColorChangeStatusEffectComponent : Component
{
    [DataField(required: true)]
    public Color Color;
}

[RegisterComponent, NetworkedComponent]
public sealed partial class ColorChangeComponent : Component;

[Serializable, NetSerializable]
public enum ColorStatusEffectVisuals : byte
{
    Color,
}
