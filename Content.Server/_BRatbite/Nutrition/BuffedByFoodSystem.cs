using Content.Shared._BRatbite.Nutrition;
using Content.Shared.Nutrition;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Nutrition;

public sealed partial class BuffedByFoodSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly StatusEffectsSystem _statusEffectsSystem = default!;
    [Dependency] private readonly SharedCookedFoodSystem _cookedFoodSystem = default!;
    [Dependency] private readonly IPrototypeManager _proto = default!;
    private static readonly int _maxActivatedBuffs = 2;
    private static readonly TimeSpan _buffDuration = TimeSpan.FromMinutes(30);

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BuffedByFoodComponent, MapInitEvent>(OnBuffedInit);
        SubscribeLocalEvent<CookedFoodComponent, FullyEatenEvent>(OnFoodEaten);
    }

    public override void Update(float _)
    {
        var query = EntityQueryEnumerator<BuffedByFoodComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.ActivatedBuffs.PopNext(out var item)) continue;
            CleanupStatusEffects(ref item);
        }
    }

    private void CleanupStatusEffects(ref Buff item)
    { 
        foreach (var ent in item.Ents)
            QueueDel(ent); // Deleting the entities will call appropriate events
    }

    private void OnFoodEaten(Entity<CookedFoodComponent> ent, ref FullyEatenEvent args)
    {    
        if (!TryComp<BuffedByFoodComponent>(args.User, out var buffedByFood)) return;
        var buff = new Buff(new());
        var freshness = _proto.Index(_cookedFoodSystem.GetFreshnessLevel(ent));
        foreach (var effect in freshness.Effects)
        {
            // TODO: If we know a status effect can't be applied twice, we can simply add the time
            if(_statusEffectsSystem.TryAddStatusEffect(args.User, effect, out var effectUid))
                buff.Ents.Add(effectUid.Value); // TODO: check why C# is stupid
        }
        if (buffedByFood.ActivatedBuffs.Push(buff, out var old))
            CleanupStatusEffects(ref old);
        ScaleBuffs((args.User, buffedByFood));
    }

    private void ScaleBuffs(Entity<BuffedByFoodComponent> ent)
    {
        for (int i = 1; i < ent.Comp.ActivatedBuffs.Count; i++)
        {
            var buff = ent.Comp.ActivatedBuffs[i];
            foreach (var entity in buff.Ents)
            {
                if (!TryComp<StatusEffectScaleComponent>(entity, out var statusScale)) continue;
                // Each buff will be half as effective as the previous one
                var ev = new StatusEffectScaleEvent(1f / (1 << i), statusScale.Scale, ent.Owner);
                RaiseLocalEvent(entity, ref ev);
            }
        }
    }

    private void OnBuffedInit(Entity<BuffedByFoodComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.ActivatedBuffs = new (_maxActivatedBuffs, _buffDuration, _timing);
    }
}
