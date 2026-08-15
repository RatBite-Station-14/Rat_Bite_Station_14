using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;

namespace Content.Shared._BRatbite.Access;

public abstract partial class SharedEmergencyAccessSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmergencyAccessComponent, GetAccessTagsEvent>(OnGetAccessTags, after: [typeof(SharedAccessSystem)]);
    }

    private void OnGetAccessTags(Entity<EmergencyAccessComponent> ent, ref GetAccessTagsEvent args)
    {
        if (IsAlertLevelReached(ent))
        {
            args.Tags.UnionWith(ent.Comp.AddedTags);
            args.Tags.ExceptWith(ent.Comp.RemovedTags);
        }
    }

    protected abstract bool IsAlertLevelReached(Entity<EmergencyAccessComponent> ent);
}
