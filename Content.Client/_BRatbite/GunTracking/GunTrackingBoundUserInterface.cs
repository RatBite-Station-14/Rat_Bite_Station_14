using Content.Shared._BRatbite.GunTracking;
using Robust.Client.UserInterface;

namespace Content.Client._BRatbite.GunTracking;

public sealed partial class GunTrackingBoundUserInterface : BoundUserInterface
{
    [ViewVariables]
    private GunTrackingWindow? _window;

    public GunTrackingBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        EntityUid? gridUid = null;
        if (EntMan.TryGetComponent<TransformComponent>(Owner, out var xform))
        {
            gridUid = xform.GridUid;
        }
        _window = this.CreateWindow<GunTrackingWindow>();
        _window.SetMap(gridUid);
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        base.UpdateState(state);
        if (state is not GunTrackingComputerState trackingState) return;
        _window?.PopulateGuns(trackingState.Guns);
    }
}
