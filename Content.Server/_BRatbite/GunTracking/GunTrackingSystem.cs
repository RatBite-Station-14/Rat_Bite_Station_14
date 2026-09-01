using System.Numerics;
using Content.Shared._BRatbite.GunTracking;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.GunTracking;

public sealed partial class GunTrackingSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _uiSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly TransformSystem _transform = default!;
    private readonly static TimeSpan _updateInterval = TimeSpan.FromSeconds(5);
    private TimeSpan _nextUpdate;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunTrackingComputerComponent, BoundUIOpenedEvent>(OnUIOpened);
    }

    public override void Update(float _)
    {
        if (_timing.CurTime < _nextUpdate) return;
        _nextUpdate = _timing.CurTime + _updateInterval;
        var eq = EntityQueryEnumerator<GunTrackingComputerComponent>();
        List<GunStatus>? statuses = null;
        while (eq.MoveNext(out var uid, out var gunTrackingComp))
        {
            if (!_uiSystem.IsAnyUiOpen(uid)) continue;
            if (statuses is null) statuses = GetGunStatuses();
            UpdateUserInterface((uid, gunTrackingComp), statuses);
        }
    }

    private void UpdateUserInterface(Entity<GunTrackingComputerComponent> ent, List<GunStatus>? gunStatuses = null)
    {
        _uiSystem.SetUiState(ent.Owner, GunTrackingUIKey.Key, new GunTrackingComputerState(gunStatuses ?? GetGunStatuses()));
    }


    private List<GunStatus> GetGunStatuses()
    {
        var gunStatuses = new List<GunStatus>();
        var eq = EntityQueryEnumerator<TransformComponent, GunTrackerComponent>();
        while (eq.MoveNext(out var uid, out var transform, out var _))
        {
            EntityCoordinates coordinates;
            if (transform.GridUid != null)
            {
                coordinates = new EntityCoordinates(transform.GridUid.Value,
                                                  Vector2.Transform(_transform.GetWorldPosition(transform),
                                                  _transform.GetInvWorldMatrix(Transform(transform.GridUid.Value))));
            }
            else if (transform.MapUid != null)
            {
                coordinates = new EntityCoordinates(transform.MapUid.Value,
                                                  _transform.GetWorldPosition(transform));
            }
            else
            {
                continue;
            }

            gunStatuses.Add(new (GetNetEntity(uid), "test", GetNetCoordinates(coordinates)));
        }
        return gunStatuses;
    }

    private void OnUIOpened(Entity<GunTrackingComputerComponent> ent, ref BoundUIOpenedEvent args)
    {
        UpdateUserInterface(ent);
    }
}
