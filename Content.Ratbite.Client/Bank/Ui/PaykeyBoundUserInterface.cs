using Content.Ratbite.Shared.Bank;
using Robust.Shared.Utility;

namespace Content.Ratbite.Client.Bank.Ui;

public sealed partial class PaykeyBoundUserInterface : BoundUserInterface
{
    private PaykeyWindow? _window;
    private List<NetEntity> _cachedBanks = new();

    public PaykeyBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    { }

    protected override void Open()
    {
        base.Open();

        _window = new PaykeyWindow();
        _window.OnClose += Close;

        _window.OnAmountConfirmed += (account, password, bankId, amount) =>
        {
            if (!_cachedBanks.TryGetValue(bankId, out var bank))
                return;

            SendMessage(new BankSendToOOCMessage(account, password, bank, amount));
        };

        _window.OpenCentered();
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);

        if (state is not PaykeyInterfaceState paykeyState)
            return;

        _cachedBanks = paykeyState.Banks;
        _window?.UpdateAvailableBanks(_cachedBanks);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing) return;

        _window?.Dispose();
    }
}
