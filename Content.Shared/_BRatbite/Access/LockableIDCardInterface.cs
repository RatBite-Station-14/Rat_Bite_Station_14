using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Access;

[Serializable, NetSerializable]
public enum LockableIDUiKey : byte
{
    Key,
}

[Serializable, NetSerializable]
public sealed class LockableIDSendPasswordMessage : BoundUserInterfaceMessage
{
    public readonly string Password;

    public LockableIDSendPasswordMessage(string password)
    {
        Password = password;
    }
}
