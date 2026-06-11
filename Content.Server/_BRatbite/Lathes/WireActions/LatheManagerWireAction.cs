using Content.Shared.Lathe;
using Content.Server.Wires;
using Content.Shared.Wires;
using Content.Shared._BRatbite.Lathe;
using Content.Shared._BRatbite.Machines;

namespace Content.Server._BRatbite.Lathes.WireActions;

public sealed partial class LatheManagerWireAction : ComponentWireAction<ManagerLatheRecipesComponent>
{
    public override Color Color { get; set; } = Color.Green;
    public override string Name { get; set; } = "wire-name-lathe-manager";

    public override StatusLightState? GetLightState(Wire wire, ManagerLatheRecipesComponent comp)
    {
        return comp.Cut ? StatusLightState.BlinkingSlow : StatusLightState.On;
    }

    public override object? StatusKey { get; } = BoltableMachineWireStatus.Manager;

    public override bool Cut(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        return EntityManager.System<ManagerLatheSystem>().ManagerWireCut(user, wire, comp);
    }

    public override bool Mend(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        return EntityManager.System<ManagerLatheSystem>().ManagerWireMend(user, wire, comp);
    }

    public override void Pulse(EntityUid user, Wire wire, ManagerLatheRecipesComponent comp)
    {
        EntityManager.System<ManagerLatheSystem>().ManagerWirePulse(user, wire, comp);
    }
}
