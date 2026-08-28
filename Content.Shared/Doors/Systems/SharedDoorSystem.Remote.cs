// Ratbite file

using Content.Shared.Access.Components;
using Content.Shared.Database;
using Content.Shared.Doors.Components;
using Content.Shared.Electrocution;
using Content.Shared.Remotes.Components;

namespace Content.Shared.Doors.Systems;

public abstract partial class SharedDoorSystem
{
    [Dependency] private readonly SharedElectrocutionSystem _electrify = default!;

    private void InitDoorRemotes()
    {
        SubscribeLocalEvent<DoorComponent, DoorRemoteUsedEvent>(OnDoorRemoteUse);
    }

    private void OnDoorRemoteUse(Entity<DoorComponent> ent, ref DoorRemoteUsedEvent args)
    {
        var isAirlock = TryComp<AirlockComponent>(args.Target, out var airlockComp);

        args.Handled = true;

        if (TryComp<AccessReaderComponent>(ent, out var accessComponent)
            && !HasAccess(ent, args.AccessTarget, ent.Comp, accessComponent))
        {
            if (isAirlock)
                Deny(ent.Owner, ent.Comp, user: args.User, predicted: true);

            Popup.PopupClient(Loc.GetString("door-remote-denied"), args.User, args.User);
            return;
        }

        switch (args.Mode)
        {
            case OperatingMode.OpenClose:
                if (TryToggleDoor(ent, ent.Comp, user: args.User, predicted: true))
                    _adminLog.Add(LogType.Action,
                                     LogImpact.Medium,
                                     $"{ToPrettyString(args.User):player} used {ToPrettyString(args.Remote)} on {ToPrettyString(ent)}: {ent.Comp.State}");
                break;
            case OperatingMode.ToggleBolts:
                if (TryComp<DoorBoltComponent>(ent, out var boltsComp))
                {
                    if (!boltsComp.BoltWireCut)
                    {
                        SetBoltsDown((ent, boltsComp), !boltsComp.BoltsDown, user: args.User, predicted: true);
                        _adminLog.Add(LogType.Action,
                                         LogImpact.Medium,
                                         $"{ToPrettyString(args.User):player} used {ToPrettyString(args.Remote)} on {ToPrettyString(ent)} to {(boltsComp.BoltsDown ? "" : "un")}bolt it");
                    }
                }

                break;
                // This is now handled by EmergencyComponentSystem
            case OperatingMode.ToggleEmergencyAccess:
                break;
            case OperatingMode.ToggleOvercharge:
                if (TryComp<ElectrifiedComponent>(ent, out var eletrifiedComp))
                {
                    _electrify.SetElectrified((ent, eletrifiedComp), !eletrifiedComp.Enabled);
                    var soundToPlay = eletrifiedComp.Enabled
                        ? eletrifiedComp.AirlockElectrifyEnabled
                        : eletrifiedComp.AirlockElectrifyDisabled;
                    Audio.PlayLocal(soundToPlay, ent, args.User);
                    _adminLog.Add(LogType.Action,
                                     LogImpact.Medium,
                                     $"{ToPrettyString(args.User):player} used {ToPrettyString(args.Remote)} on {ToPrettyString(ent)} to {(eletrifiedComp.Enabled ? "" : "un")}electrify it");
                }

                break;
        }
    }
}
