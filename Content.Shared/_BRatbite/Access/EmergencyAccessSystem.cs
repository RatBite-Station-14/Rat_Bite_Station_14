using Content.Shared.Access.Systems;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Remotes.Components;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._BRatbite.Access;

public sealed partial class EmergencyAccessSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPowerReceiverSystem _power = default!;
    [Dependency] private readonly ISharedAdminLogManager _adminLogger = default!;
    [Dependency] private readonly AccessReaderSystem _accessReaderSystem = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<EmergencyAccessComponent, DoorRemoteUsedEvent>(OnDoorRemoteUse);
    }

    public void SetEmergencyAccess(Entity<EmergencyAccessComponent> ent, bool value, EntityUid? user = null, bool predicted = false)
    {
        if (!_power.IsPowered(ent.Owner)) return;
        if (ent.Comp.EmergencyAccess == value)
            return;

        ent.Comp.EmergencyAccess = value;
        Dirty(ent, ent.Comp);

        var sound = ent.Comp.EmergencyAccess ? ent.Comp.EmergencyOnSound : ent.Comp.EmergencyOffSound;
        if (predicted)
            _audio.PlayPredicted(sound, ent, user: user);
        else
            _audio.PlayPvs(sound, ent);
        var ev = new EmergencyAccessChangedEvent(value);
        RaiseLocalEvent(ent, ref ev);
    }

    private void OnDoorRemoteUse(Entity<EmergencyAccessComponent> ent, ref DoorRemoteUsedEvent args)
    {
        if (!_accessReaderSystem.IsAllowed(args.AccessTarget, args.Target))
        {
            _popup.PopupClient(Loc.GetString("door-remote-denied"), args.User, args.User);
            return;
        }
        if (args.Mode != OperatingMode.ToggleEmergencyAccess) return;
        SetEmergencyAccess(ent, !ent.Comp.EmergencyAccess, user: args.User, predicted: true);
        _adminLogger.Add(LogType.Action,
                         LogImpact.Medium,
                         $"{ToPrettyString(args.User):player} used {ToPrettyString(args.Remote)} on {ToPrettyString(ent)} to set emergency access {(ent.Comp.EmergencyAccess ? "on" : "off")}"); 
    }
}
