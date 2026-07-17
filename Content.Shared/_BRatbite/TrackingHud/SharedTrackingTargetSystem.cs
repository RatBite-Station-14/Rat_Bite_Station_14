using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.TrackingHud;

public abstract partial class SharedTrackingTargetSystem : EntitySystem
{
    private int _counter = 0;
    private Dictionary<string, TrackingTarget> _activeTargets = new();

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TargetTrackerComponent, MapInitEvent>(OnMapInit);
    }

    // Add to a single target, this does not relay to other ones
    public void AddTarget(Entity<TargetTrackerComponent?> ent, string id, TrackingTarget target, TimeSpan? deleteAfter = null)
    {
        if (!Resolve(ent.Owner, ref ent.Comp))
            return;

        if (!ent.Comp.Targets.ContainsKey(id))
        {
            ent.Comp.Targets.Add(id, target);
            if (deleteAfter is { } after)
            {
                Timer.Spawn(after, () => RemoveTarget(ent, id));
            }
            Dirty(ent);
        }
    }

    public void RemoveTarget(Entity<TargetTrackerComponent?> ent, string id)
    {
        if (TerminatingOrDeleted(ent.Owner))
            return;

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

        var id = (_counter++).ToString();
        if (_activeTargets.ContainsKey(id)) return id;
        _activeTargets.Add(id, target);
        var a = EntityQueryEnumerator<TargetTrackerComponent>();
        while (a.MoveNext(out var uid, out var trackerComponent))
        {
            // Handle deleteAfter globally
            AddTarget((uid, trackerComponent), id, target, deleteAfter: null);
        }
        if (deleteAfter is { } after)
        {
            Timer.Spawn(after, () => RemoveFromAllTargets(id));
        }
        return id;
    }

    public void RemoveFromAllTargets(string id)
    {
        if (!_activeTargets.ContainsKey(id)) return;
        _activeTargets.Remove(id);
        var a = EntityQueryEnumerator<TargetTrackerComponent>();
        while (a.MoveNext(out var uid, out var trackerComponent))
        {
            RemoveTarget((uid, trackerComponent), id);
        }
    }

    private void OnMapInit(Entity<TargetTrackerComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Targets = new(_activeTargets);
    }
}
