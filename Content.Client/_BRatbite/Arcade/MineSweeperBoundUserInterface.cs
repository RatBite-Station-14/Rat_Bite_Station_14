using Content.Shared._BRatbite.Arcade;
using Robust.Client.UserInterface;

namespace Content.Client._BRatbite.Arcade;

public sealed partial class MineSweeperBoundUserInterface : BoundUserInterface
{
    private MineSweeperWindow? _window;

    public MineSweeperBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        _window = this.CreateWindow<MineSweeperWindow>();
        _window.OnCellClicked += (x, y, flag) => SendMessage(new CellClickedBUIMessage(x, y, flag));
        _window.OnRestartClicked += () => SendMessage(new RestartGameBUIMessage());
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not MineSweeperBUIState s) return;
        _window?.UpdateBoard(s);
    }
}
