// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunComponentUpgradeComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
