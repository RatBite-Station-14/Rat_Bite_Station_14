// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class UpgradeableGunComponent : Component
{
    [DataField]
    public string UpgradesContainerId = "upgrades";

    [DataField]
    public EntityWhitelist Whitelist = new();

    [DataField]
    public int? MaxUpgradeCount;

    [DataField]
    public int MaxUpgradeCapacity = 100;
}
