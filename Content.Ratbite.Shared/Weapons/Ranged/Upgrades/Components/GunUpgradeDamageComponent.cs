// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeDamageComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new();

    /// <summary>
    /// How much of damage applies if the weapon shoots pellets (shotgun)
    /// </summary>
    [DataField]
    public float PelletModifier = 1f;
}
