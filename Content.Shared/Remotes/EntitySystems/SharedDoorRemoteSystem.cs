// SPDX-License-Identifier: MIT

using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Remotes.Components;
using Robust.Shared.Serialization;
using Robust.Shared.Timing;

namespace Content.Shared.Remotes.EntitySystems;

// Ratbite: This was refactored to remove the hard coding on doors and
// make it more generic using an event. This is to allow other
// machines like lathes and vending machines to be affected by the
// remote too
public abstract class SharedDoorRemoteSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _powerReceiver = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] protected readonly IGameTiming Timing = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<DoorRemoteComponent, DoorRemoteModeChangeMessage>(OnDoorRemoteModeChange);
        SubscribeLocalEvent<DoorRemoteComponent, BeforeRangedInteractEvent>(OnBeforeInteract);
    }

    private void OnDoorRemoteModeChange(Entity<DoorRemoteComponent> ent, ref DoorRemoteModeChangeMessage args)
    {
        ent.Comp.Mode = args.Mode;
        Dirty(ent);
    }

    private void OnBeforeInteract(Entity<DoorRemoteComponent> entity, ref BeforeRangedInteractEvent args)
    {
        if (!Timing.IsFirstTimePredicted)
            return;
        if (args.Target is null || args.Handled) return;
        if (!_examine.InRangeUnOccluded(args.User,
                args.Target.Value,
                SharedInteractionSystem.MaxRaycastRange,
                                        null)) return;

        var accessTarget = args.Used;
        // This covers the accesses the REMOTE has, and is not effected by the user's ID card.
        if (entity.Comp.IncludeUserAccess) // Allows some door remotes to inherit the user's access.
        {
            accessTarget = args.User;
            // This covers the accesses the USER has, which always includes the remote's access since holding a remote acts like holding an ID card.
        }

        if (!_powerReceiver.IsPowered(args.Target.Value))
        {
            _popup.PopupClient(Loc.GetString("door-remote-no-power"), args.User, args.User);
            return;
        }

        var ev = new DoorRemoteUsedEvent(args.Target.Value, args.User, entity, entity.Comp.Mode, accessTarget);
        RaiseLocalEvent(args.Target.Value, ref ev);
        args.Handled = ev.Handled;
    }
}

[Serializable, NetSerializable]
public sealed class DoorRemoteModeChangeMessage : BoundUserInterfaceMessage
{
    public OperatingMode Mode;
}

[Serializable, NetSerializable]
public enum DoorRemoteUiKey : byte
{
    Key
}
