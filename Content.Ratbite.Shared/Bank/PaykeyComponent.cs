// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Audio;

namespace Content.Ratbite.Shared.Bank;

[RegisterComponent, NetworkedComponent]
public sealed partial class PaykeyComponent : Component
{
    [DataField]
    public SoundSpecifier? SoundOnTransfer = new SoundPathSpecifier("/Audio/Weapons/Guns/Hits/laser_sear_wall.ogg", AudioParams.Default.WithVariation(0.05f));

    [DataField]
    public ProtoId<FranchisePrototype>? Franchise;
}
