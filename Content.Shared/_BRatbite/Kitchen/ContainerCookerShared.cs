using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Kitchen;

[Serializable, NetSerializable]
public enum ContainerCookerUiKey { Key }

[Serializable, NetSerializable]
public sealed class ContainerCookerEjectMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public sealed class ContainerCookerEjectIndexedIngredientMessage(NetEntity entityId) : BoundUserInterfaceMessage
{
    public NetEntity EntityId = entityId;
}

[Serializable, NetSerializable]
public sealed class ContainerCookerUpdateUserInterfaceState(bool busy) : BoundUserInterfaceState
{
    public bool Busy = busy;
}
