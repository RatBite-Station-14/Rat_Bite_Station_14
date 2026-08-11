using Content.Server.Damage.Systems;
using Content.Server.Mousetrap;
using Content.Shared._BRatbite.Mousetrap;
using Content.Shared.Abilities;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Mousetrap;
using Content.Shared.StepTrigger.Systems;

namespace Content.Server._BRatbite.Mousetrap;

public sealed partial class GiantMouseTrapSystem : SharedGiantMouseTrapSystem
{
    [Dependency] private readonly SharedSuicideSystem _suicideSystem = default!;
    [Dependency] private readonly MousetrapSystem _mousetrapSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GiantMouseTrapComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<GiantMouseTrapComponent, BeforeDamageUserOnTriggerEvent>(BeforeDamageOnTrigger, after: [typeof(MousetrapSystem)]);
    }

    private void OnStepTriggerAttempt(Entity<GiantMouseTrapComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Cancelled = !HasComp<AlwaysTriggerMousetrapComponent>(args.Tripper);
    }

    private void BeforeDamageOnTrigger(Entity<GiantMouseTrapComponent> ent, ref BeforeDamageUserOnTriggerEvent args)
    {
        if (!TryComp<DamageableComponent>(args.Tripper, out var damageable)) return;
        _suicideSystem.ApplyLethalDamage((args.Tripper, damageable), args.Damage);
        args.Damage *= 0;
    }

    protected override void OnInteractHand(Entity<GiantMouseTrapComponent> ent, ref InteractHandEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MousetrapComponent>(ent, out var mousetrapComp)) return;

        _mousetrapSystem.ToggleTrap((ent, mousetrapComp), args.User);

        args.Handled = true;
    }
}
