using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Arcade;

[Serializable, NetSerializable]
public sealed partial class MineSweeperBUIState : BoundUserInterfaceState
{
    public List<MineSweeperBlockState> Board;
    public int BoardWidth;
    public int BoardHeight;
    public MineSweeperGameState State;
    // Creation time to ensure all clients get the same contraband
    public TimeSpan CreationTime;

    public MineSweeperBUIState(List<MineSweeperBlockState> board, int boardWidth, int boardHeight, MineSweeperGameState state, TimeSpan creationTime)
    {
        Board = board;
        BoardWidth = boardWidth;
        BoardHeight = boardHeight;
        State = state;
        CreationTime = creationTime;
    }
}

[Serializable, NetSerializable]
public sealed partial class CellClickedBUIMessage : BoundUserInterfaceMessage
{
    public int X;
    public int Y;
    public bool Flag;

    public CellClickedBUIMessage(int x, int y, bool flag)
    {
        X = x;
        Y = y;
        Flag = flag;
    }
}

[Serializable, NetSerializable]
public sealed partial class RestartGameBUIMessage : BoundUserInterfaceMessage;

[Serializable, NetSerializable]
public enum MineSweeperUiKey
{
    Key
}
