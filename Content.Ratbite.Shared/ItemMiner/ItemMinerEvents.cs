namespace Content.Ratbite.Shared.ItemMiner;

/// <summary>
/// Raised on an item miner to check whether it should work right now.
/// </summary>
[ByRefEvent]
public record struct ItemMinerCheckEvent(bool Cancelled = false);

/// <summary>
/// Raised on an item miner when it mines an item.
/// </summary>
public sealed class ItemMinedEvent(EntityUid mined, int count) : EntityEventArgs
{
    /// <summary>
    /// The entity we have modified or created
    /// </summary>
    public readonly EntityUid Mined = mined;

    /// <summary>
    /// How much has been actually spawned or added to the stack, can be 0
    /// </summary>
    public readonly int Count = count;
}
