using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Kitchen;

[Serializable, NetSerializable]
public enum TimedCookerUiKey { Key }

[Serializable, NetSerializable]
public sealed class TimedCookerStartCookingMessage(uint selectedTimePreset) : BoundUserInterfaceMessage
{
    public uint SelectedTimePreset = selectedTimePreset;
}

[Serializable, NetSerializable]
public sealed class TimedCookerEjectMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class TimedCookerEjectIndexedIngrediantMessage(NetEntity entityId) : BoundUserInterfaceMessage
{
    public NetEntity EntityId = entityId;
}

[Serializable, NetSerializable]
public sealed class TimedCookerUpdateUserInterfaceState(
    TimeSpan? recipeEnd)
    : BoundUserInterfaceState
{
    public TimeSpan? RecipeEnd = recipeEnd;
}


[Serializable, NetSerializable]
public enum TimedCookerDataKeys
{
    DoorOpen,
    Active,
    Bloody,
}
