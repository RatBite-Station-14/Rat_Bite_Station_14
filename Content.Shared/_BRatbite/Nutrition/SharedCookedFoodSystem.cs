using Content.Shared.Examine;
using Content.Shared.Temperature.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Nutrition;

public sealed partial class SharedCookedFoodSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private int MaxFreshnessLevels;

    public override void Initialize()
    {
        base.Initialize();
        MaxFreshnessLevels = _proto.GetInstances<FoodStatusPrototype>().Count;
        SubscribeLocalEvent<CookedFoodComponent, MapInitEvent>(OnCookedFoodInit);
        SubscribeLocalEvent<CookedFoodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    private void OnCookedFoodInit(Entity<CookedFoodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastFreshnessUpdate = _timing.CurTime;
    }

    public ProtoId<FoodStatusPrototype> GetFreshnessLevel(Entity<CookedFoodComponent> ent)
    {
        var foodDecay = _proto.Index(ent.Comp.FoodDecayPrototype);
        ref var currentFreshness = ref ent.Comp.CurrentFreshness;
        // The for loop is a guard against malformed prototypes, we
        // only loop up to the maximum numbers of defined freshness levels
        for (int i = 0; i < MaxFreshnessLevels; i++)
        {
            if (!foodDecay.DecayTimes.TryGetValue(ent.Comp.CurrentFreshness, out var decayTime)) break;
            var (time, freshness) = decayTime;
            var elapsedTime = _timing.CurTime - ent.Comp.LastFreshnessUpdate;
            if (elapsedTime < time) break;
            ent.Comp.LastFreshnessUpdate += time;
            currentFreshness = freshness;
        }
        return currentFreshness;
    }

    public ProtoId<FoodStatusPrototype>? GetTemperatureStatus(Entity<CookedFoodComponent> ent)
    {
        if (!TryComp<TemperatureComponent>(ent, out var tempComp)) return null;
        float closest = 0f;
        var foodTemp = tempComp.CurrentTemperature;
        var temperatureProto = _proto.Index(ent.Comp.FoodTemperaturePrototype);
        foreach (var threshold in temperatureProto.TemperatureThresholds)
        {
            if (foodTemp >= threshold.Key && threshold.Key > closest)
                closest = threshold.Key;
        }
        return temperatureProto.TemperatureThresholds.GetValueOrDefault(closest);
    }

    private void OnPrototypeReload(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<FoodStatusPrototype>())
        {
            MaxFreshnessLevels = _proto.GetInstances<FoodStatusPrototype>().Count;
        }
    }

    private void OnExamined(Entity<CookedFoodComponent> ent, ref ExaminedEvent args)
    {
        var freshness = _proto.Index(GetFreshnessLevel(ent));
        if (freshness.ExamineText is { } examineText)
        {
            args.PushMarkup(Loc.GetString(examineText));
        }
        var temperature = _proto.Index(GetTemperatureStatus(ent));
        if (temperature?.ExamineText is { } text)
        {
            args.PushMarkup(Loc.GetString(text));
        }
    }
}
