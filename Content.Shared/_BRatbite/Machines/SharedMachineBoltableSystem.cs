using Robust.Shared.Serialization;
using Content.Shared.Popups;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared._BRatbite.Machines;
using Content.Shared.Wires;
using Robust.Shared.GameObjects;
using Robust.Shared.Audio.Systems;
using Content.Shared._BRatbite.Machines;

namespace Content.Shared._BRatbite.Machines;

public abstract class SharedMachineBoltableSystem : EntitySystem
{
    [Dependency] protected readonly SharedTransformSystem _transform = default!;
    [Dependency] protected readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BoltableMachineComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        SubscribeLocalEvent<BoltableMachineComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
    }

    private void OnAnchorAttempt(Entity<BoltableMachineComponent> ent, ref AnchorAttemptEvent args)
    {
        if (!CheckAnchorAttempt(ent, args.User))
            args.Cancel();
    }

    private void OnUnanchorAttempt(Entity<BoltableMachineComponent> ent, ref UnanchorAttemptEvent args)
    {
        if (!CheckAnchorAttempt(ent, args.User))
            args.Cancel();
    }

    private bool CheckAnchorAttempt(Entity<BoltableMachineComponent> ent, EntityUid user)
    {
        // Don't allow the thing to be anchored if bolted to the ground
        if (!ent.Comp.Bolted)
            return true;

        if (ent.Comp.AnchorFailedMessage != null)
            _popup.PopupEntity(Loc.GetString(ent.Comp.AnchorFailedMessage), ent.Owner, user);


        return false;
    }



}

[NetSerializable, Serializable]
public enum BoltableMachineWireStatus
{
    BoltIndicator,
}

