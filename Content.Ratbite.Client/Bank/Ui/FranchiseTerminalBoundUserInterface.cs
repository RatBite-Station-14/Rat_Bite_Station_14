using Content.Ratbite.Shared.Bank;

namespace Content.Ratbite.Client.Bank.Ui;

public sealed partial class FranchiseTerminalBoundUserInterface : BoundUserInterface
{
    private FranchiseTerminalWindow? _window;

    public FranchiseTerminalBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _window = new FranchiseTerminalWindow();
        _window.OnClose += Close;

        _window.OnFranchiseSelected += id => SendMessage(new FranchiseTerminalSelectMessage(id));
        _window.OnWorkerPaySet += (uid, rate) => SendMessage(new FranchiseTerminalSetWorkerPayMessage(uid, rate));
        _window.OnWorkerFired += uid => SendMessage(new FranchiseTerminalFireWorkerMessage(uid));
        _window.OnAccountConfigured += (acc, pass) => SendMessage(new FranchiseTerminalConfigureAccountMessage(acc, pass));

        _window.OpenCentered();
    }
    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not FranchiseTerminalInterfaceState cast)
            return;

        if (cast.FranchiseId is not { } franchiseId)
        {
            _window?.PopulateSelectionScreen(cast.AvailableProtos);
        }
        else
        {
            _window?.UpdateDashboard(franchiseId, cast.Workers, cast.LinkedAccount, cast.AccountBalance);
        }
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;
        _window?.Dispose();
    }
}
