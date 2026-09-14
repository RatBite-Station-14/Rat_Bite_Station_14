using Content.Goobstation.Shared.Xenobiology.Components;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs;

namespace Content.Goobstation.Shared.Xenobiology.Systems;

// Ratbite: This handles capping slime breeding.
public partial class XenobiologySystem
{
    private Dictionary<EntityUid, int> _cachedStationSlimeCount = [];
    private int _slimeCountCap; // Set by cvar
    private TimeSpan _nextSlimeCountCacheUpdate = TimeSpan.Zero;
    private readonly TimeSpan _slimeCountCacheDelay = TimeSpan.FromSeconds(10);
    private Dictionary<EntityUid, TimeSpan> _nextCapPopupTime = [];
    private readonly TimeSpan _capPopupCooldown = TimeSpan.FromSeconds(30);

    private void UpdateSlimeCountCache()
    {
        if (_net.IsClient)
            return;

        if (_gameTiming.CurTime < _nextSlimeCountCacheUpdate)
            return;

        _nextSlimeCountCacheUpdate = _gameTiming.CurTime + _slimeCountCacheDelay;
        _cachedStationSlimeCount.Clear();

        var query = EntityQueryEnumerator<SlimeComponent, MobStateComponent, TransformComponent>();
        while (query.MoveNext(out _, out _, out var mobState, out var xform))
        {
            if (xform.GridUid is not { } grid)
                continue;

            if (mobState.CurrentState == MobState.Dead)
                continue;

            _cachedStationSlimeCount.TryGetValue(grid, out var count);
            _cachedStationSlimeCount[grid] = count + 1;
        }
    }
}
