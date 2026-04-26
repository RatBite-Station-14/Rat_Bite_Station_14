// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeComponentsComponent : Component
{
    [DataField]
    public ComponentRegistry Components = new();
}
