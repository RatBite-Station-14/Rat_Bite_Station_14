namespace Content.Ratbite.Shared.Bank;

[Serializable, NetSerializable]
public enum FranchiseTerminalUiKey : byte
{
    Key
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalInterfaceState(ProtoId<FranchisePrototype>? franchiseId, List<ProtoId<FranchisePrototype>> availableProtos, List<FranchiseTerminalWorkerData> workers, string linkedAccount, int accountBalance) : BoundUserInterfaceState
{
    public ProtoId<FranchisePrototype>? FranchiseId = franchiseId;
    public List<ProtoId<FranchisePrototype>> AvailableProtos = availableProtos;
    public List<FranchiseTerminalWorkerData> Workers = workers;
    public string LinkedAccount = linkedAccount;
    public int AccountBalance = accountBalance;
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalWorkerData(NetUserId userId, string name, int payRate)
{
    public NetUserId UserId = userId;
    public string Name = name;
    public int PayRate = payRate;
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalSelectMessage(string prototypeId) : BoundUserInterfaceMessage
{
    public string PrototypeId = prototypeId;
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalSetWorkerPayMessage(NetUserId workerId, int newPayRate) : BoundUserInterfaceMessage
{
    public NetUserId WorkerId = workerId;
    public int NewPayRate = newPayRate;
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalFireWorkerMessage(NetUserId workerId) : BoundUserInterfaceMessage
{
    public NetUserId WorkerId = workerId;
}

[Serializable, NetSerializable]
public sealed class FranchiseTerminalConfigureAccountMessage(string account, string password) : BoundUserInterfaceMessage
{
    public string Account = account;
    public string Password = password;
}
