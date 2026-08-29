using Content.Server.Power.EntitySystems;
using Content.Server.Wires;
using Content.Shared._BRatbite.Machines;

namespace Content.Server._BRatbite.Machines;

public sealed class BoltableMachineSystem : SharedBoltableMachineSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;


    [Dependency] private readonly PowerReceiverSystem _power = default!;

    public bool BoltWireCut(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        comp.BoltedWireCut = true;
        if (!_power.IsPowered(wire.Owner))
            return true;
        // Only play the audio if it was unbolted before
        if (comp.Bolted == false)
            _audio.PlayPvs(comp.BoltSound, wire.Owner);
        comp.Bolted = true;
        Dirty(wire.Owner, comp);
        return true;
    }

    public bool BoltWireMend(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        comp.BoltedWireCut = false;
        Dirty(wire.Owner, comp);
        // Mending bolt wire does nothing
        return true;
    }

    public void BoltWirePulse(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        if (!_power.IsPowered(wire.Owner))
            return;
        ToggleBolts((wire.Owner, comp));
    }
}
