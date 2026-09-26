using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Nutrition.Foodifiers;

public abstract partial class OvertimeStatusEffectSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;

    private TimeSpan _nextUpdate;
    protected TimeSpan _updateFrequency = TimeSpan.FromSeconds(1);
    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float _)
    {
        base.Update(_);
        if (_timing.CurTime < _nextUpdate)
            return;

        Tick(_timing.CurTime - _nextUpdate + _updateFrequency);
        _nextUpdate = _timing.CurTime + _updateFrequency;
    }

    protected abstract void Tick(TimeSpan elapsedTime);
}
