using Content.Server.Fluids.EntitySystems;
using Content.Server.Stack;
using Content.Shared.Chemistry.Components;
using Content.Shared.Electrocution;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Chemistry;

public sealed partial class ChemialSystem : EntitySystem
{
    [Dependency] private readonly PuddleSystem _puddleSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StackSystem _stackSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<ChemialComponent, ElectrocutionAttemptEvent>(OnElectrocuted);
    }

    private void OnElectrocuted(Entity<ChemialComponent> ent, ref ElectrocutionAttemptEvent args)
    {
        if (args.Cancelled) return;
        if (ent.Comp.Reagents.Count == 0) return;
        var reagentProto = _random.Pick(ent.Comp.Reagents);
        var solution = new Solution(reagentProto, ent.Comp.SpillQuantity * _stackSystem.GetCount(ent.Owner));
        _puddleSystem.TrySpillAt(ent, solution, out _);
    }
}
