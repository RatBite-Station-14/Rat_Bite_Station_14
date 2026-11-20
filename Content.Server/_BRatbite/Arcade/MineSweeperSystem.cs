using System.Linq;
using Content.Shared._BRatbite.Arcade;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.Arcade;

public sealed partial class MineSweeperSystem : EntitySystem
{
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MineSweeperComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<MineSweeperComponent, BoundUIOpenedEvent>(OnBUIOpen);
        SubscribeLocalEvent<MineSweeperComponent, CellClickedBUIMessage>(OnClickCell);
        SubscribeLocalEvent<MineSweeperComponent, RestartGameBUIMessage>((ent, ref _) => { ClearBoard(ent); UpdateBUIState(ent); });
    }

    private void OnMapInit(Entity<MineSweeperComponent> ent, ref MapInitEvent args)
    {
        var (width, height) = (ent.Comp.BoardWidth, ent.Comp.BoardHeight);
        ent.Comp.Board.Capacity = width * height;

        ClearBoard(ent);
    }

    private void ClearBoard(Entity<MineSweeperComponent> ent)
    {
        ent.Comp.State = MineSweeperGameState.EmptyBoard;
        ent.Comp.UncoveredCells = 0;
        ent.Comp.Board.Clear();
        ent.Comp.CreationTime = _timing.CurTime;
        var (width, height) = (ent.Comp.BoardWidth, ent.Comp.BoardHeight);
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                ent.Comp.Board.Add(MineSweeperBlockState.Blank | MineSweeperBlockState.Covered);
            }
        }
    }

    private void UncoverBoard(Entity<MineSweeperComponent> ent)
    {
        for (var i = 0; i < ent.Comp.Board.Count; i++)
            ent.Comp.Board[i] &= ~(MineSweeperBlockState.Covered | MineSweeperBlockState.Flag);
    }

    private void AddMines(Entity<MineSweeperComponent> ent)
    {
        var (width, height) = (ent.Comp.BoardWidth, ent.Comp.BoardHeight);
        // Just in case someone messes with the yaml
        if (ent.Comp.MinesToAdd > width * height) throw new ArgumentOutOfRangeException("More mines to add than there are cells");
        var minesAdded = 0;
        while (minesAdded < ent.Comp.MinesToAdd)
        {
            var x = _random.Next(0, width);
            var y = _random.Next(0, height);
            var cell = ent.Comp.Board[x + y * width];
            if (!cell.IsCovered()) continue;
            if (cell.Uncovered() == MineSweeperBlockState.Mine) continue;
            ent.Comp.Board[x + y * width] |= MineSweeperBlockState.Mine;
            minesAdded++;
        }
        for (var y = 0; y < height; y++)
        {
            for (var x = 0; x < width; x++)
            {
                var cell = ent.Comp.Board[x + y * width];
                if (cell.Uncovered() == MineSweeperBlockState.Mine) continue;
                int neighborMines = 0;
                for (var j = -1; j <= 1; j++)
                {
                    for (var i = -1; i <= 1; i++)
                    {
                        if (i == 0 && j == 0) continue;
                        if (x + i < 0 || y + j < 0) continue;
                        if (x + i >= width || y + j >= height) continue;
                        if (ent.Comp.Board[(x + i) + (y + j) * width].Uncovered() == MineSweeperBlockState.Mine)
                            neighborMines++;
                    }
                }
                ent.Comp.Board[x + y * width] |= (MineSweeperBlockState) neighborMines;
            }
        }
    }

    private void OnBUIOpen(Entity<MineSweeperComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateBUIState(ent);
    }

    private void UpdateBUIState(Entity<MineSweeperComponent> ent)
    {
        _ui.SetUiState(ent.Owner, MineSweeperUiKey.Key,
                       new MineSweeperBUIState(
                           ent.Comp.Board.Select((c) => c.StrippedFromPrivateInfo()).ToList(),
                           ent.Comp.BoardWidth, ent.Comp.BoardHeight,
                           ent.Comp.State,
                           ent.Comp.CreationTime
                       )
        );
    }

    private void OnClickCell(Entity<MineSweeperComponent> ent, ref CellClickedBUIMessage args)
    {
        var (width, height) = (ent.Comp.BoardWidth, ent.Comp.BoardHeight);
        if (args.X < 0 || args.X >= width) return;
        if (args.Y < 0 || args.Y >= height) return;
        if (!ent.Comp.Board[args.X + args.Y * ent.Comp.BoardWidth].IsCovered()) return;
        switch (ent.Comp.State)
        {
            case MineSweeperGameState.EmptyBoard:
                if (args.Flag) return;
                ent.Comp.Board[args.X + args.Y * width] &= ~MineSweeperBlockState.Covered;
                ent.Comp.UncoveredCells++;
                AddMines(ent);
                UncoverCell(ent, args.X, args.Y);
                ent.Comp.State = MineSweeperGameState.Playing;
                break;
            case MineSweeperGameState.Playing:
                if (args.Flag)
                    ent.Comp.Board[args.X + args.Y * width] ^= MineSweeperBlockState.Flag;
                else
                {
                    if (ent.Comp.Board[args.X + args.Y * width].IsFlagged()) return;
                    UncoverCell(ent, args.X, args.Y);
                    if (ent.Comp.Board[args.X + args.Y * width] == MineSweeperBlockState.Mine) { ent.Comp.State = MineSweeperGameState.Lost; UncoverBoard(ent); }
                    else if (ent.Comp.UncoveredCells >= width * height - ent.Comp.MinesToAdd)
                    { ent.Comp.State = MineSweeperGameState.Won; UncoverBoard(ent); }
                }
                break;
            default:
                return;
        }

        UpdateBUIState(ent);
    }

    private void UncoverCell(Entity<MineSweeperComponent> ent, int initialX, int initialY)
    {
        var (w, h) = (ent.Comp.BoardWidth, ent.Comp.BoardHeight);
        if (ent.Comp.Board[initialX + initialY * w].IsCovered()) ent.Comp.UncoveredCells++;
        ent.Comp.Board[initialX + initialY * w] &= ~MineSweeperBlockState.Covered;

        var cellsToUncover = new List<(int, int)>() { (initialX, initialY) };
        while (cellsToUncover.Count != 0)
        {
            var (x, y) = cellsToUncover.Pop();
            if (ent.Comp.Board[x + y * w].IsCovered()) ent.Comp.UncoveredCells++;
            if (ent.Comp.Board[x + y * w] != MineSweeperBlockState.Blank) continue;
            for (var j = -1; j <= 1; j++)
            {
                for (var i = -1; i <= 1; i++)
                {
                    if (i == 0 && j == 0) continue;
                    if (x + i < 0 || y + j < 0) continue;
                    if (x + i >= w || y + j >= h) continue;
                    if (ent.Comp.Board[(x + i) + (y + j) * w].IsCovered())
                    {
                        ent.Comp.Board[(x + i) + (y + j) * w] &= ~MineSweeperBlockState.Covered;
                        ent.Comp.UncoveredCells++;
                        cellsToUncover.Add((x + i, y + j));
                    }

                }
            }
        }
    }
}
