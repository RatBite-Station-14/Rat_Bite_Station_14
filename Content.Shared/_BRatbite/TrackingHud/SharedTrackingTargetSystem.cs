using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.TrackingHud;

public abstract partial class SharedTrackingTargetSystem : EntitySystem
{
    private int _counter = 0;
    public override void Initialize()
    {
        base.Initialize();
    }

    public void AddTarget(Entity<TargetTrackerComponent?> ent, string id, TrackingTarget target, TimeSpan? deleteAfter = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (!ent.Comp.Targets.ContainsKey(id))
        {
            ent.Comp.Targets.Add(id, target);
            if (deleteAfter is { } after)
            {
                Timer.Spawn(after, () =>
                {
                    if (TerminatingOrDeleted(ent.Owner))
                        return;
                    RemoveTarget(ent, id);
                });
            }
            Dirty(ent);
        }
    }

    public void RemoveTarget(Entity<TargetTrackerComponent?> ent, string id)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (ent.Comp.Targets.ContainsKey(id))
        {
            ent.Comp.Targets.Remove(id);
            Dirty(ent);
        }
    }

    // Add target to all entities that have TrackerTargetComponent. Returns the id to remove this
    // tracker later
    public string AddTargetToAllEntities(TrackingTarget target, TimeSpan? deleteAfter = null)
    {
        var a = EntityQueryEnumerator<TargetTrackerComponent>();
        while (a.MoveNext(out var uid, out var trackerComponent))
        {
            AddTarget((uid, trackerComponent), (_counter++).ToString(), target, deleteAfter);
        }
        return _counter.ToString();
    }
}
