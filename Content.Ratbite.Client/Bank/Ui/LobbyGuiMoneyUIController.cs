using Content.Client.Lobby;
using Content.Client.Lobby.UI;
using Content.Ratbite.Shared.Bank;
using Robust.Client.State;
using Robust.Client.UserInterface.Controllers;
using Robust.LoaderApi;

namespace Content.Ratbite.Client.Bank.Ui;

public sealed partial class LobbyGuiMoneyUIController : UIController, IOnStateChanged<LobbyState>
{
    [Dependency] private IClientPreferencesManager _pref = default!;
    [Dependency] private IStateManager _state = default!;
    [Dependency] private IClientNetManager _net = default!;

    private LobbyGuiMoney? _moneyWidget;

    public override void Initialize()
    {
        base.Initialize();

        _net.RegisterNetMessage<MsgBankBalanceResponse>(HandleBalanceReceived);
        _net.RegisterNetMessage<MsgRequestBankBalance>();
        _net.RegisterNetMessage<MsgUpdateLobbyBringAmount>();
    }

    public void OnStateEntered(LobbyState state)
    {
        if (state.Lobby is not { } lobby)
            return;

        SpawnWindow(lobby);
    }

    public void OnStateExited(LobbyState state)
    {
        if (state.Lobby is not { } lobby)
            return;
    }

    private void HandleBalanceReceived(MsgBankBalanceResponse message)
    {
        _moneyWidget?.SetWalletState(message.Balance, 0);
    }

    private void SpawnWindow(LobbyGui lobby)
    {
        _moneyWidget = new LobbyGuiMoney();
        _moneyWidget.OnBringAmountChanged += HandleBringAmountChanged;
        lobby.RoundstartMoney.RemoveAllChildren();
        lobby.RoundstartMoney.AddChild(_moneyWidget);

        _net.ClientSendMessage(new MsgRequestBankBalance());
        _net.ClientSendMessage(new MsgUpdateLobbyBringAmount { Balance = 0 });
    }

    private void HandleBringAmountChanged(int amount)
    {
        _net.ClientSendMessage(new MsgUpdateLobbyBringAmount { Balance = amount });
    }
}
