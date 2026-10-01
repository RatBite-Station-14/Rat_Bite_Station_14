using Content.Shared._BRatbite.Nutrition.Components;
using Content.Shared.Atmos;
using Content.Shared.Examine;
using Content.Shared.Kitchen;
using Content.Shared.Temperature;
using Content.Shared.Temperature.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Nutrition;

public sealed partial class SharedCookedFoodSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _proto = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly static ProtoId<FoodTemperaturePrototype> MicrowavedPrototype = "ReheatedTemperature";
    private int MaxFreshnessLevels;

    public override void Initialize()
    {
        base.Initialize();
        MaxFreshnessLevels = _proto.GetInstances<FoodStatusPrototype>().Count;
        SubscribeLocalEvent<CookedFoodComponent, MapInitEvent>(OnCookedFoodInit);
        SubscribeLocalEvent<CookedFoodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<CookedFoodComponent, BeingMicrowavedEvent>(OnMicrowaved);
        SubscribeLocalEvent<CookedFoodComponent, OnTemperatureChangeEvent>((ent, ref _) => GetFreshnessLevel(ent));
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    private void OnCookedFoodInit(Entity<CookedFoodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ElapsedTime = default;
        ent.Comp.LastFoodDecayUpdate = _timing.CurTime;
        Dirty(ent);
    }


    // https://www.desmos.com/calculator/30ruvlb4rl
    private float GetTemperatureScale(float? temperature)
    {
        if (temperature is not { } t) return 1f;
        if (t < Atmospherics.T0C) return 0f;
        return (MathF.Atan((t - 293) / 5) / MathF.PI) + (1 / 2);
    }

    public ProtoId<FoodStatusPrototype> GetFreshnessLevel(Entity<CookedFoodComponent> ent)
    {
        var curTime = _timing.CurTime;
        var foodDecay = _proto.Index(ent.Comp.FoodDecayPrototype);
        ref var currentFreshness = ref ent.Comp.CurrentFreshness;
        var elapsedTime = curTime - ent.Comp.LastFoodDecayUpdate;
        var temp = CompOrNull<TemperatureComponent>(ent)?.CurrentTemperature;
        ent.Comp.LastFoodDecayUpdate = curTime;
        var tempScale = GetTemperatureScale(temp);
        if (tempScale == 0f) return currentFreshness;
        ent.Comp.ElapsedTime += elapsedTime * tempScale;
        // The for loop is a guard against malformed prototypes, we
        // only loop up to the maximum numbers of defined freshness levels
        for (int i = 0; i < MaxFreshnessLevels; i++)
        {
            if (!foodDecay.DecayTimes.TryGetValue(ent.Comp.CurrentFreshness, out var decayTime)) break;
            var (time, freshness) = decayTime;
            if (ent.Comp.ElapsedTime < time) break;
            ent.Comp.ElapsedTime -= time;
            currentFreshness = freshness;
        }
        Dirty(ent);
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

    public void AddStatusEffect(Entity<CookedFoodComponent> ent,  EntProtoId statusEffect)
    {
        // TODO: is this the best way of doing this?? - verdant
        ent.Comp.StatusEffectProto.Add(statusEffect);
    }

    private void OnMicrowaved(Entity<CookedFoodComponent> ent, ref BeingMicrowavedEvent args)
    {
        ent.Comp.FoodTemperaturePrototype = MicrowavedPrototype;
    }
}
