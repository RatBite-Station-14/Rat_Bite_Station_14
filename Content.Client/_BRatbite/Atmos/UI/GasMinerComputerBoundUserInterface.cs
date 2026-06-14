using Content.Shared._BRatbite.Atmos;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;

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
        //	_window.SetEntity(Owner);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (_window == null || state is not GasMinerComputerBoundUserInterfaceState gasMinerState)
            return;
        _window.Populate(gasMinerState);
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        if (!disposing)
            return;
        _window?.Dispose();
    }
}
