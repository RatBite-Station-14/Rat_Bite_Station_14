using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Remotes.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._BRatbite.Machines;

public abstract class SharedBoltableMachineSystem : EntitySystem
{
    [Dependency] protected readonly SharedTransformSystem _transform = default!;
    [Dependency] protected readonly SharedPopupSystem _popup = default!;
    [Dependency] protected readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLog = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<BoltableMachineComponent, AnchorAttemptEvent>(OnAnchorAttempt);
        SubscribeLocalEvent<BoltableMachineComponent, UnanchorAttemptEvent>(OnUnanchorAttempt);
        SubscribeLocalEvent<BoltableMachineComponent, AnchorStateChangedEvent>(OnAnchorStateChange);
        SubscribeLocalEvent<BoltableMachineComponent, DoorRemoteUsedEvent>(OnDoorRemoteUsed);
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
            _popup.PopupClient(Loc.GetString(ent.Comp.AnchorFailedMessage), ent.Owner, user);


        return false;
    }

    private void OnAnchorStateChange(Entity<BoltableMachineComponent> ent, ref AnchorStateChangedEvent args)
    {
        // Unbolt if the anchor state changes
        if (!args.Anchored)
            SetBolts(ent, false);
    }

    public void SetBolts(Entity<BoltableMachineComponent> ent, bool value, EntityUid? user = null)
    {
        if (ent.Comp.Bolted == value) return;
        ent.Comp.Bolted = value;
        Dirty(ent);
        PlayAudio(ent, user);
    }

    public void ToggleBolts(Entity<BoltableMachineComponent> ent, EntityUid? user = null)
    {
        SetBolts(ent, !ent.Comp.Bolted, user);
    }

    private void OnDoorRemoteUsed(Entity<BoltableMachineComponent> ent, ref DoorRemoteUsedEvent args)
    {
        if (!_accessReaderSystem.IsAllowed(args.AccessTarget, args.Target))
        {
            _popup.PopupClient(Loc.GetString("door-remote-denied"), args.User, args.User);
            return;
        }
        if (args.Mode == OperatingMode.ToggleBolts && !ent.Comp.BoltedWireCut)
        {
            ToggleBolts(ent, args.User);
            _adminLog.Add(LogType.Action,
                          LogImpact.Medium,
                          $"{ToPrettyString(args.User):player} used {ToPrettyString(args.Remote)} on {ToPrettyString(ent)} to {(ent.Comp.Bolted ? "" : "un")}bolt it");
            args.Handled = true;
        }
    }

    protected void PlayAudio(Entity<BoltableMachineComponent> ent, EntityUid? user = null)
    {
        _audio.PlayPredicted(ent.Comp.Bolted ? ent.Comp.BoltSound : ent.Comp.UnboltSound, ent.Owner, user);
    }

}

[NetSerializable, Serializable]
public enum BoltableMachineWireStatus
{
    BoltIndicator,
}

