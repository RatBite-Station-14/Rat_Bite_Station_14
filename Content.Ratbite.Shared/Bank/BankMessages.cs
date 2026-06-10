using Lidgren.Network;

namespace Content.Ratbite.Shared.Bank;

/// <summary>
/// Requests shitcoins from server.
/// </summary>
[Serializable, NetSerializable]
public sealed class MsgRequestBankBalance : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer) { }
    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer) { }
}

/// <summary>
/// Recieve shitcoin count from server.
/// </summary>
public sealed class MsgBankBalanceResponse : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public int Balance;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Balance = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(Balance);
    }
}

/// <summary>
/// Request amount of shitcoins to send to server to bring into round.
/// </summary>
public sealed class MsgUpdateLobbyBringAmount : NetMessage
{
    public override MsgGroups MsgGroup => MsgGroups.Command;

    public int Balance;

    public override void ReadFromBuffer(NetIncomingMessage buffer, IRobustSerializer serializer)
    {
        Balance = buffer.ReadVariableInt32();
    }

    public override void WriteToBuffer(NetOutgoingMessage buffer, IRobustSerializer serializer)
    {
        buffer.WriteVariableInt32(Balance);
    }
}
