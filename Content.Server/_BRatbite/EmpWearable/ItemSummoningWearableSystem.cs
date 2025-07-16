using Content.Server.Emp;
using Content.Shared._BRatbite.EmpGlove;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Containers;
using Robust.Shared.Containers;

namespace Content.Server._BRatbite.EmpWearable;

/// <inheritdoc/>
public sealed class ItemSummoningWearableSystem : SharedItemSummoningWearableSystem
{
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ItemSummoningWearableComponent, ItemSummonActionEvent>(OnSummonEmp);
    }

    private void OnSummonEmp(Entity<ItemSummoningWearableComponent> ent, ref ItemSummonActionEvent args)
    {
        Log.Info("Summon Event Ran");
        if (ent.Comp.SummonedEntity != EntityUid.Invalid)
        {
            Log.Info("Already Existed");
            QueueDel(ent.Comp.SummonedEntity);
            ent.Comp.SummonedEntity = EntityUid.Invalid;
            _actionsSystem.SetToggled(args.Action, false);
            return;
        }

        if (!_hands.TryGetEmptyHand(args.Performer, out var emptyHand))
            return;
        Log.Info("Got Hand");

        var st = Spawn(ent.Comp.SummonEntity, Transform(ent).Coordinates);

        if (!_hands.TryPickup(args.Performer, st, emptyHand, animate: false))
        {
            QueueDel(ent.Comp.SummonedEntity);
            return;
        }
        Log.Info("Pickup");

        if (!TryComp<SummonedItemDespawnComponent>(st, out var stComp))
            return;

        ent.Comp.SummonedEntity = st;
        stComp.Creator = ent;

        _actionsSystem.SetToggled(args.Action, true);

        Log.Info("Summoned Emp");
    }
}
