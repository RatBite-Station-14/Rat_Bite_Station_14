using Content.Ratbite.Shared.EmpGlove;
using Content.Shared.Actions;
using Content.Shared.Interaction;

namespace Content.Ratbite.Server.EmpWearable;

/// <summary>
/// This handles despawning a summoned item after it has been used.
/// Also manages starting the action cooldown after item usage.
/// </summary>
public sealed partial class SummonedItemDespawnSystem : EntitySystem
{
    [Dependency] private SharedActionsSystem _actionsSystem = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SummonedItemDespawnComponent, AfterInteractEvent>(OnAfterInteract);
    }

    private void OnAfterInteract(Entity<SummonedItemDespawnComponent> ent, ref AfterInteractEvent args)
    {
        if (!TryComp<ItemSummoningWearableComponent>(ent.Comp.Creator, out var creatorComp))
        {
            QueueDel(ent);
            return;
        }

        if (!args.Handled)
            return;

        _actionsSystem.SetCooldown(creatorComp.ActionEntity, creatorComp.CooldownAfterUse);
        _actionsSystem.SetToggled(creatorComp.ActionEntity, false);
        creatorComp.SummonedEntity = EntityUid.Invalid;
        QueueDel(ent);
    }
}
