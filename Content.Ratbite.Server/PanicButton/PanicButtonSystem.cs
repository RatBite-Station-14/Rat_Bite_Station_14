// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Ratbite.Shared.PanicButton;
using Content.Server.Pinpointer;
using Content.Server.Radio.EntitySystems;
using Content.Shared.Interaction.Events;
using Content.Shared.Radio;
using Content.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Ratbite.Server.PanicButton;

public sealed partial class PanicButtonSystem : EntitySystem
{
    [Dependency] private NavMapSystem _nav = default!;
    [Dependency] private RadioSystem _radio = default!;
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private UseDelaySystem _useDelaySystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PanicButtonComponent, UseInHandEvent>(OnButtonPressed);
    }

    private void OnButtonPressed(Entity<PanicButtonComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        EnsureComp<UseDelayComponent>(ent.Owner, out var useDelay);
        if (_useDelaySystem.IsDelayed((ent.Owner, useDelay)))
            return;

        var comp = ent.Comp;
        var uid = ent.Owner;

        if (_useDelaySystem.IsDelayed((ent.Owner, useDelay)))
            return;

        _useDelaySystem.TryResetDelay((uid, useDelay));

        // Gets location of the implant
        var posText = FormattedMessage.RemoveMarkupOrThrow(_nav.GetNearestBeaconString(uid));
        var distressMessage = Loc.GetString(comp.DistressMessage, ("position", posText));

        _radio.SendRadioMessage(uid, distressMessage, _prototypeManager.Index<RadioChannelPrototype>(comp.RadioChannel), uid);

        args.Handled = true;
    }
}
