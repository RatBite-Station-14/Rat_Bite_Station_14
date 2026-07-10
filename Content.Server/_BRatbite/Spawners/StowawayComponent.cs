using Content.Shared.Inventory;

namespace Content.Server._BRatbite.Spawners;

[RegisterComponent]
public sealed partial class StowawayComponent : Component
{
    [DataField]
    public float SlotDeletionChance = 0.4f;

    [DataField]
    public SlotFlags SlotsToRemove = SlotFlags.IDCARD | SlotFlags.BACK;
}
