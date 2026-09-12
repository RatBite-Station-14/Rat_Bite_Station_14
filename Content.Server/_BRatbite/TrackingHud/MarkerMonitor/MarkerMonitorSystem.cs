using System.Linq;
using Content.Shared._BRatbite.TrackingHud;
using Content.Shared._BRatbite.TrackingHud.MarkerMonitor;
using Robust.Server.GameObjects;

namespace Content.Server._BRatbite.TrackingHud.MarkerMonitor;

public sealed partial class MarkerMonitorSystem : SharedMarkerMonitorSystem
{
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MarkerMonitorComputerComponent, BoundUIOpenedEvent>(OnBUIOpened);
    }

    private void OnBUIOpened(Entity<MarkerMonitorComputerComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateBUIState(ent);
    }

    private void UpdateBUIState(Entity<MarkerMonitorComputerComponent> ent)
    {
        if (!TryComp<TargetTrackerComponent>(ent, out var targetTracker)) return;

        _ui.SetUiState(ent.Owner, MarkerMonitorUIKey.Key, new MarkerBoundUserInterfaceState(targetTracker.Targets.Values.ToList()));
    }

    protected override void OnAddMarker(Entity<MarkerMonitorComputerComponent> ent, ref AddMarkerBoundUserInterfaceMessage args)
    {
        base.OnAddMarker(ent, ref args);
        UpdateBUIState(ent);
    }
}
