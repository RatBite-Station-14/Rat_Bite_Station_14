using Content.Shared.VendingMachines;
using Robust.Shared.Physics.Events;
using Content.Shared.Construction.EntitySystems;
using Content.Shared.Stunnable;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;
using Content.Shared.Damage.Prototypes;
using Content.Shared.Verbs;
using Content.Shared.DoAfter;
using Robust.Shared.Serialization;
using Content.Shared.VendingMachines;
using System.Numerics;

namespace Content.Server._BRatbite.VendingMachines;

public sealed partial class VendingMachineTippingSystem : EntitySystem
{
    [Dependency] private readonly AnchorableSystem _anchorableSystem = default!;
    [Dependency] private readonly SharedTransformSystem _transformSystem = default!;
    [Dependency] private readonly SharedStunSystem _stunSystem = default!;
    [Dependency] private readonly DamageableSystem _damageableSystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedVendingMachineSystem _vendingMachineSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<VendingMachineComponent, StartCollideEvent>(OnCollision);
        SubscribeLocalEvent<VendingMachineComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<VendingMachineComponent, VendingMachineUntipEvent>(OnUntipMachine);
    }

    private void OnCollision(Entity<VendingMachineComponent> ent, ref StartCollideEvent args)
    {
        if (ent.Comp.Tipped) return;
        var force = Vector2.Dot(args.OurBody.LinearVelocity - args.OtherBody.LinearVelocity, args.WorldNormal) * args.OtherBody.Mass;
        if (force >= 600)
        {
            ent.Comp.Tipped = true;
            _transformSystem.Unanchor(ent.Owner, Transform(ent.Owner));
            _vendingMachineSystem.TryUpdateVisualState((ent.Owner, ent.Comp));

            _stunSystem.TryParalyze(args.OtherEntity, TimeSpan.FromSeconds(2), true);
            _stunSystem.TrySeeingStars(args.OtherEntity);
            _damageableSystem.TryChangeDamage(args.OtherEntity, new DamageSpecifier(_prototypeManager.Index<DamageTypePrototype>("Blunt"), 15));
        }
    }

    private void AddAlternativeVerbs(Entity<VendingMachineComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || !ent.Comp.Tipped)
            return;
        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("vending-machine-untip"),
            Priority = 1,
            Act = () =>
            {
                var dargs = new DoAfterArgs(EntityManager, user, TimeSpan.FromSeconds(20), new VendingMachineUntipEvent(), ent.Owner)
                {
                    BreakOnMove = true,
                    BreakOnDamage = true,
                    BreakOnWeightlessMove = true,
                    NeedHand = true,
                    DuplicateCondition = DuplicateConditions.SameEvent,
                };
                _doAfter.TryStartDoAfter(dargs);
            }
        });
    }

    private void OnUntipMachine(Entity<VendingMachineComponent> ent, ref VendingMachineUntipEvent args)
    {
        if (args.Cancelled || args.Handled) return;
        ent.Comp.Tipped = false;
        _vendingMachineSystem.TryUpdateVisualState((ent.Owner, ent.Comp));
    }
}

