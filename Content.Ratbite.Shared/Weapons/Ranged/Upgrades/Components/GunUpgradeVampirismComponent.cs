// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

/// <summary>
/// A <see cref="GunUpgradeComponent"/> for increasing the damage of a gun's projectile.
/// </summary>
[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeVampirismComponent : Component
{
    [DataField]
    public DamageSpecifier DamageOnHit = new();
}

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class ProjectileVampirismComponent : Component
{
    [DataField]
    public DamageSpecifier DamageOnHit = new();
}
