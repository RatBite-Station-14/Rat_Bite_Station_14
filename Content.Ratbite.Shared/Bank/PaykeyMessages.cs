namespace Content.Ratbite.Shared.Bank;

[Serializable, NetSerializable]
public enum PaykeyUiKey : byte
{
    Key
}

/// <summary>
/// Fired from server to client to send available banks.
/// </summary>
[Serializable, NetSerializable]
public sealed class PaykeyInterfaceState(List<NetEntity> banks) : BoundUserInterfaceState
{
    public List<NetEntity> Banks = banks;
}
