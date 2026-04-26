// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the speed of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeSpeedComponent : Component
{
    [DataField]
    public float Coefficient = 1;
}
