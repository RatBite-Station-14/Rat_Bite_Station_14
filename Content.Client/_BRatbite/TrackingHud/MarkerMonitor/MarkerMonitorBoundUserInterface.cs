using Content.Shared._BRatbite.TrackingHud.MarkerMonitor;
using Robust.Client.UserInterface;

namespace Content.Client._BRatbite.TrackingHud.MarkerMonitor;

public sealed partial class MarkerMonitorBoundUserInterface : BoundUserInterface
{
    private MarkerMonitorWindow? _window;

    public MarkerMonitorBoundUserInterface(EntityUid owner, Enum uiKey) : base(owner, uiKey)
    {
    }

    protected override void Open()
    {
        base.Open();
        var gridUid = EntMan.GetComponentOrNull<TransformComponent>(Owner)?.GridUid;
        _window = this.CreateWindow<MarkerMonitorWindow>();
        _window.SetMap(gridUid);
        _window.ClickedOnMapAction += (position, marker, color) => { SendPredictedMessage(new AddMarkerBoundUserInterfaceMessage(position, marker, color)); };
    }

    protected override void UpdateState(BoundUserInterfaceState state)
    {
        if (state is not MarkerBoundUserInterfaceState s) return;
        _window?.UpdateMarkers(s.Targets);
    }
}
