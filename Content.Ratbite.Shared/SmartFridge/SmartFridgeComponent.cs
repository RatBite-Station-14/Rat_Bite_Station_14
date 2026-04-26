// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Whitelist;
using Robust.Shared.Audio;

namespace Content.Ratbite.Shared.SmartFridge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(true)]
public sealed partial class SmartFridgeComponent : Component
{
    [DataField]
    public string Container = "smart_fridge_inventory";

    [DataField]
    public EntityWhitelist? Whitelist;

    [DataField]
    public EntityWhitelist? Blacklist;

    [DataField]
    public SoundSpecifier? InsertSound = new SoundPathSpecifier("/Audio/Weapons/Guns/MagIn/revolver_magin.ogg");

    [DataField, AutoNetworkedField]
    public List<SmartFridgeEntry> Entries = new();

    [DataField, AutoNetworkedField]
    public Dictionary<SmartFridgeEntry, List<NetEntity>> ContainedEntries = new();

    /// <summary>
    ///     Sound that plays when ejecting an item
    /// </summary>
    [DataField]
    public SoundSpecifier SoundVend = new SoundPathSpecifier("/Audio/Machines/machine_vend.ogg")
    {
        Params = new AudioParams
        {
            Volume = -4f,
            Variation = 0.15f
        }
    };

    /// <summary>
    ///     Sound that plays when an item can't be ejected
    /// </summary>
    [DataField]
    public SoundSpecifier SoundDeny = new SoundPathSpecifier("/Audio/Machines/custom_deny.ogg");

    // Frontier:
    /// <summary>
    /// The maximum number of entities that can be stored in the fridge
    /// </summary>
    [DataField]
    public int MaxContainedCount = 300;
    // End Frontier
}

[DataDefinition]
public partial struct SmartFridgeEntry
{
    [DataField]
    public string Name;

    public SmartFridgeEntry(string name)
    {
        Name = name;
    }
}

[Serializable, NetSerializable]
public enum SmartFridgeUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed partial class SmartFridgeDispenseItemMessage(SmartFridgeEntry entry) : BoundUserInterfaceMessage
{
    public SmartFridgeEntry Entry = entry;
}
