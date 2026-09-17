using Content.Shared._EinsteinEngines.Silicon.Components;
using Content.Shared.Temperature;

namespace Content.Shared._BRatbite.Silicon;

public sealed partial class TemperatureBatteryDrainSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TemperatureBatteryDrainComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<TemperatureBatteryDrainComponent, OnTemperatureChangeEvent>(OnTemperatureChanged);
    }

    private void OnStartup(Entity<TemperatureBatteryDrainComponent> ent, ref ComponentStartup args)
    {
        if (!TryComp<SiliconComponent>(ent, out var siliconComp))
        {
            RemCompDeferred<TemperatureBatteryDrainComponent>(ent);
            return;
        }
        ent.Comp.OriginalDrain = siliconComp.DrainPerSecond;
    }

    private void OnTemperatureChanged(Entity<TemperatureBatteryDrainComponent> ent, ref OnTemperatureChangeEvent args)
    {
        if (!TryComp<SiliconComponent>(ent, out var siliconComp))
        {
            RemCompDeferred<TemperatureBatteryDrainComponent>(ent);
            return;
        }
        var drainMultiplier = 1 + MathF.Pow(ent.Comp.OptimalTemperature - args.CurrentTemperature, 2) / 1000;
        siliconComp.DrainPerSecond = drainMultiplier * ent.Comp.OriginalDrain;
    }
}
