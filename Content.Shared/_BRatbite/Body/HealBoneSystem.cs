using Content.Shared.Body.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Robust.Shared.Utility;
using Robust.Shared.Timing;
using System.Linq;

namespace Content.Shared._BRatbite.Body;

public sealed class HealBoneSystem : EntitySystem
{
    [Dependency] private readonly TraumaSystem _traumaSystem = default!;
    [Dependency] private readonly WoundSystem _woundSystem = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_timing.IsFirstTimePredicted)
            return;
        foreach (var comp in EntityManager.EntityQuery<HealBoneComponent>())
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;
            comp.NextUpdate = _timing.CurTime + comp.UpdateRate;

            HealBones(comp.Owner, -comp.BoneHealingPerSecond * (float) comp.UpdateRate.TotalSeconds);
        }
    }


    private void HealBones(EntityUid e, float amount)
    {
        if (!TryComp<BodyComponent>(e, out var body)
            || body.RootContainer.ContainedEntities.FirstOrNull() is not { } root)
            return;

        var woundables = _woundSystem.GetAllWoundableChildren(root).ToList();
        foreach (var woundable in woundables)
        {
            if (woundable.Comp.Bone.ContainedEntities.FirstOrNull() is not { } bone)
                continue;

            _traumaSystem.ApplyDamageToBone(bone, amount / woundables.Count);
        }
    }
}
