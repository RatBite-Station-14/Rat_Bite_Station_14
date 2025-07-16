using Content.Shared.Actions;

namespace Content.Shared._BRatbite.EmpGlove;

/// <summary>
/// This handles...
/// </summary>
public abstract  class SharedItemSummoningWearableSystem : EntitySystem
{
    [Dependency] protected readonly SharedActionsSystem _actionsSystem = default!;
    [Dependency] private readonly ActionContainerSystem _actionContainer = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<ItemSummoningWearableComponent, GetItemActionsEvent>(OnGetActions);
        SubscribeLocalEvent<ItemSummoningWearableComponent, MapInitEvent>(OnMapInit);
    }

    private void OnMapInit(EntityUid uid, ItemSummoningWearableComponent component, MapInitEvent args)
    {
        _actionContainer.EnsureAction(uid, ref component.ActionEntity, component.Action);
        Dirty(uid, component);
    }

    private void OnGetActions(EntityUid uid, ItemSummoningWearableComponent component, GetItemActionsEvent args)
    {
        if (!args.SlotFlags.HasValue)
            return;

        if (!component.ValidSlots.HasFlag(args.SlotFlags))
            return;

        args.AddAction(component.ActionEntity);
    }
}
