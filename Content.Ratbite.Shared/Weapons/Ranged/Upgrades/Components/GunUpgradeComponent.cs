// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Tag;

namespace Content.Ratbite.Shared.Weapons.Ranged.Upgrades.Components;

[RegisterComponent, NetworkedComponent, Access(typeof(SharedGunUpgradeSystem))]
public sealed partial class GunUpgradeComponent : Component
{
    [DataField]
    public List<ProtoId<TagPrototype>> Tags = new();

    [DataField]
    public LocId ExamineText;

    [DataField]
    public int CapacityCost = 30; // By default drains 30% of the capacity.

    /// <summary>
    /// If true, only one such upgrade can be inserted into a gun.
    /// </summary>
    [DataField]
    public bool Unique;
}
