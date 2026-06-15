using Content.Shared._BRatbite.Atmos;
using Robust.Client.UserInterface;

namespace Content.Client._BRatbite.Atmos.UI;

public sealed class GasMinerComputerBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GasMinerComputerWindow? _window;

    public GasMinerComputerBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<GasMinerComputerWindow>();
        _window.OnClose += () => _window = null;
        _window.OnMessageSend += (args) =>
        {
            SendMessage(new GasMinerSetExchangeMessage(args.entity, args.exchangeValue, args.state));
        };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not GasMinerComputerBoundUserInterfaceState gasMinerState)
            return;
        _window.Populate(gasMinerState);
    }
}
