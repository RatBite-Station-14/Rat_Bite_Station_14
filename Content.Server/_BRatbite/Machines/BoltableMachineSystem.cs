using Content.Server.Popups;
using Content.Server.Wires;
using Content.Shared.Construction.Components;
using Content.Shared.Database;
using Content.Shared._BRatbite.Machines;
using Content.Shared.Wires;
using Robust.Server.GameObjects;
using Robust.Shared.Audio.Systems;
using Content.Shared._BRatbite.Machines;
using Content.Server.Power.EntitySystems;

namespace Content.Server._BRatbite.Machines;

public sealed class BoltableMachineSystem : SharedMachineBoltableSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly PowerReceiverSystem _power = default!;

    public bool BoltWireCut(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        if (!_power.IsPowered(comp.Owner))
            return true;
        // Only play the audio if it was unbolted before
        if (comp.Bolted == false)
            _audio.PlayPvs(comp.BoltSound, wire.Owner);
        comp.Bolted = true;

        return true;
    }

    public bool BoltWireMend(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        // Mending bolt wire does nothing
        return true;
    }

    public void BoltWirePulse(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        if (!_power.IsPowered(comp.Owner))
            return;
        comp.Bolted = !comp.Bolted;
        if (comp.Bolted)
        {
            _audio.PlayPvs(comp.BoltSound, wire.Owner);
        }
        else
        {
            _audio.PlayPvs(comp.UnboltSound, wire.Owner);
        }
    }
}
