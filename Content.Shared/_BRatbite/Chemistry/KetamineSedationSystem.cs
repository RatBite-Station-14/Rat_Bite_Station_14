using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Buckle.Components;
using Content.Shared.Cuffs.Components;
using Content.Shared.Cuffs;
using Content.Shared.Inventory.Events;

namespace Content.Shared._BRatbite.Chemistry;

public sealed class KetamineSedationSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solutions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<KetamineSedationComponent, GetDoAfterDelayMultiplierEvent>(OnGetDoAfterDelayMultiplier);
        SubscribeLocalEvent<UnbuckleAttemptEvent>(OnUnbuckleAttempt);
        SubscribeLocalEvent<UnCuffDoAfterEvent>(OnUncuffDoAfter);
        SubscribeLocalEvent<UnstrapAttemptEvent>(OnUnstrapAttempt);
        SubscribeLocalEvent<UncuffAttemptEvent>(OnUncuffAttempt);
        SubscribeLocalEvent<IsUnequippingAttemptEvent>(OnIsUnequippingAttempt);
    }

    private bool HasActiveKetamine(EntityUid uid)
    {
        var charcoalAmount = _solutions.GetTotalPrototypeQuantity(uid, "Charcoal").Float();
        if (charcoalAmount >= 10f)
            return false;

        if (HasComp<KetamineSedationComponent>(uid))
            return true;

        return _solutions.GetTotalPrototypeQuantity(uid, "Ketamine").Float() > 0.01f;
    }

    private void OnGetDoAfterDelayMultiplier(Entity<KetamineSedationComponent> ent, ref GetDoAfterDelayMultiplierEvent args)
    {
        args.Multiplier *= ent.Comp.DoAfterDelayMultiplier;

        var charcoalAmount = _solutions.GetTotalPrototypeQuantity(ent.Owner, "Charcoal").Float();
        if (charcoalAmount >= 10f)
            return;

        var ketamineAmount = _solutions.GetTotalPrototypeQuantity(ent.Owner, "Ketamine").Float();
        if (ketamineAmount <= 0.01f)
            return;

        // 1u ketamine => 1 minute scaling factor, capped at 15x.
        args.Multiplier *= MathF.Min(ketamineAmount, 15f);
    }

    private void OnUncuffAttempt(ref UncuffAttemptEvent args)
    {
        if (args.Cancelled || args.User != args.Target)
            return;

        if (!HasActiveKetamine(args.User))
            return;

        args.Cancelled = true;
    }

    private void OnUnbuckleAttempt(ref UnbuckleAttemptEvent args)
    {
        if (args.Cancelled || args.User == null)
            return;

        if (args.User != args.Buckle.Owner)
            return;

        if (!HasActiveKetamine(args.User.Value))
            return;

        args.Cancelled = true;
    }

    private void OnUnstrapAttempt(ref UnstrapAttemptEvent args)
    {
        if (args.Cancelled || args.User == null)
            return;

        if (args.User != args.Buckle.Owner)
            return;

        if (!HasActiveKetamine(args.User.Value))
            return;

        args.Cancelled = true;
    }

    private void OnUncuffDoAfter(UnCuffDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Args.Target is not { } target)
            return;

        var user = args.Args.User;
        if (user != target)
            return;

        if (!HasActiveKetamine(user))
            return;

        // Block the final completion step for self-unrestraining while sedated.
        args.Handled = true;
    }

    private void OnIsUnequippingAttempt(IsUnequippingAttemptEvent args)
    {
        if (args.Cancelled)
            return;

        if (args.Unequipee != args.UnEquipTarget)
            return;

        if (!HasActiveKetamine(args.Unequipee))
            return;

        // Straightjacket / restraint-like items can be removed through inventory unequip paths.
        if (!HasComp<HandcuffComponent>(args.Equipment))
            return;

        args.Cancel();
    }

}
