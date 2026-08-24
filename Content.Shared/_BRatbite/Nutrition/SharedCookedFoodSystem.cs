using Content.Shared.Examine;
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
        MaxFreshnessLevels = _proto.GetInstances<FoodFreshnessPrototype>().Count;
        SubscribeLocalEvent<CookedFoodComponent, MapInitEvent>(OnCookedFoodInit);
        SubscribeLocalEvent<CookedFoodComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<PrototypesReloadedEventArgs>(OnPrototypeReload);
    }

    private void OnCookedFoodInit(Entity<CookedFoodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.LastFreshnessUpdate = _timing.CurTime;
    }

    public ProtoId<FoodFreshnessPrototype> GetFreshnessLevel(Entity<CookedFoodComponent> ent)
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

    private void OnPrototypeReload(PrototypesReloadedEventArgs args)
    {
        if (args.WasModified<FoodFreshnessPrototype>())
        {
            MaxFreshnessLevels = _proto.GetInstances<FoodFreshnessPrototype>().Count;
        }
    }

    private void OnExamined(Entity<CookedFoodComponent> ent, ref ExaminedEvent args)
    {
        var freshness = _proto.Index(GetFreshnessLevel(ent));
        if (freshness.ExamineText is { } examineText)
        {
            args.PushMarkup(Loc.GetString(examineText));
        }
    }
}
