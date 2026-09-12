using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.TrackingHud.MarkerMonitor;

public abstract partial class SharedMarkerMonitorSystem : EntitySystem
{
    [Dependency] private readonly SharedTrackingTargetSystem _trackingTargetSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MarkerMonitorComputerComponent, AddMarkerBoundUserInterfaceMessage>(OnAddMarker);
    }

    protected virtual void OnAddMarker(Entity<MarkerMonitorComputerComponent> ent, ref AddMarkerBoundUserInterfaceMessage args)
    {

        if (!_timing.IsFirstTimePredicted) return;
        if (!_proto.TryIndex(args.Marker, out var marker) || marker.HideFromMonitor) return;
        var mapId = _transformSystem.GetMapCoordinates(ent).MapId;
        _trackingTargetSystem.AddTargetToAllEntities(new TrackingTarget {
                TargetLocation = args.TargetLocation,
                MapId = mapId,
                Channels = ListeningChannels.SECURITY,
                PinColor = args.Color,
                MarkerPrototype = args.Marker,
            }, deleteAfter: TimeSpan.FromSeconds(10));
    }
}
