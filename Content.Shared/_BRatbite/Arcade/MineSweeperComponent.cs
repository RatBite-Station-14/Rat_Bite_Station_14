using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._BRatbite.Arcade;

[RegisterComponent]
public sealed partial class MineSweeperComponent : Component
{
    [ViewVariables]
    public List<MineSweeperBlockState> Board = new();

    [DataField]
    public int BoardWidth = 32;

    [DataField]
    public int BoardHeight = 19;

    [DataField]
    public int MinesToAdd = 110;

    [ViewVariables]
    public MineSweeperGameState State = MineSweeperGameState.EmptyBoard;

    [ViewVariables]
    public int UncoveredCells = 0;

    [ViewVariables]
    public TimeSpan CreationTime;
}

[Serializable, NetSerializable]
public enum MineSweeperBlockState : byte
{
    Blank = 0,
    One = 1,
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Mine,
    // We OR Covered and Flag together with others
    Flag = 1 << 4,
    Covered = 1 << 5,
}

[Serializable, NetSerializable]
public enum MineSweeperGameState : byte
{
    EmptyBoard,
    Playing,
    Lost,
    Won,
}

public static class MineSweeperBlockStateExtensions
{
    public static MineSweeperBlockState Uncovered(this MineSweeperBlockState self)
    {
        return self & ~MineSweeperBlockState.Covered;
    }

    public static MineSweeperBlockState StrippedFromPrivateInfo(this MineSweeperBlockState self)
    {
        if ((self & MineSweeperBlockState.Flag) != 0) return MineSweeperBlockState.Flag;
        if ((self & MineSweeperBlockState.Covered) != 0) return MineSweeperBlockState.Covered;
        return self;
    }

    public static bool IsCovered(this MineSweeperBlockState self)
    {
        return (self & MineSweeperBlockState.Covered) != 0;
    }

    public static bool IsFlagged(this MineSweeperBlockState self)
    {
        return (self & MineSweeperBlockState.Flag) != 0;
    }
}

[Prototype]
[Serializable, NetSerializable]
// Prototype for the cell state.  If more exist for the same state, a
// random one will be used
public sealed partial class MineSweeperCellPrototype : IPrototype
{
    [IdDataField]
    public string ID { get; private set; } = default!;

    [DataField(required: true)]
    public MineSweeperBlockState State;

    [DataField(required: true)]
    public SpriteSpecifier Icon = default!;
}
