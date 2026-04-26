// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Shared.Weapons;

/// <summary>
///     Handles attachment display and behavior on weapons.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class WeaponAttachmentComponent : Component
{
    public const string BayonetSlotId = "bayonet";
    public const string LightSlotId = "light";
    [DataField(required: true)]
    public EntProtoId LightActionPrototype;

    [DataField, AutoNetworkedField]
    public bool LightAttached;

    [DataField, AutoNetworkedField]
    public bool BayonetAttached;

    [DataField, AutoNetworkedField]
    public bool LightOn;

    [DataField, AutoNetworkedField]
    public EntityUid? ToggleLightAction;
}

public enum WeaponVisualLayers : byte
{
    Base,
    Bayonet,
    FlightOff,
    FlightOn
}
