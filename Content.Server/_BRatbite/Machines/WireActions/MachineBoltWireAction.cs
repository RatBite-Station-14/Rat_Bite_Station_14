using Content.Shared._BRatbite.Machines;
using Content.Server._BRatbite.Machines;
using Content.Server.Wires;
using Content.Shared.Wires;

namespace Content.Server._BRatbite.Machines.WireActions;

public sealed partial class MachineBoltWireAction : ComponentWireAction<BoltableMachineComponent>
{
    public override Color Color { get; set; } = Color.Red;
    public override string Name { get; set; } = "wire-name-lathe-bolt";

    public override StatusLightState? GetLightState(Wire wire, BoltableMachineComponent comp)
    {
        return comp.Bolted ? StatusLightState.On : StatusLightState.Off;
    }

    public override object? StatusKey { get; } = BoltableMachineWireStatus.BoltIndicator;

    public override bool Cut(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        return EntityManager.System<BoltableMachineSystem>().BoltWireCut(user, wire, comp);
    }

    public override bool Mend(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        return EntityManager.System<BoltableMachineSystem>().BoltWireMend(user, wire, comp);
    }

    public override void Pulse(EntityUid user, Wire wire, BoltableMachineComponent comp)
    {
        EntityManager.System<BoltableMachineSystem>().BoltWirePulse(user, wire, comp);
    }
}
