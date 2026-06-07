// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Storage;

namespace Content.Ratbite.Server.OpenTriggeredStorageFill;

/// <summary>
/// This is used for storing an item prototype to be inserted into a container when the trigger is activated. This is deleted from the entity after the item is inserted.
/// </summary>
[RegisterComponent]
public sealed partial class OpenTriggeredStorageFillComponent : Component
{
    [DataField("contents")] public List<EntitySpawnEntry> Contents = new();
}
