using Content.Server.Damage.Systems;
using Content.Shared._BRatbite.Mousetrap;
using Content.Shared.Abilities;
using Content.Shared.Chat;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Mousetrap;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Trigger.Systems;

namespace Content.Server._BRatbite.Mousetrap;

public sealed partial class GiantMouseTrapSystem : SharedGiantMouseTrapSystem
{
    [Dependency] private readonly SharedSuicideSystem _suicideSystem = default!;
    [Dependency] private readonly ItemToggleSystem _itemToggle = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GiantMouseTrapComponent, StepTriggerAttemptEvent>(OnStepTriggerAttempt);
        SubscribeLocalEvent<GiantMouseTrapComponent, BeforeDamageOnTriggerEvent>(BeforeDamageOnTrigger, after: [typeof(MousetrapSystem)]);
    }

    private void OnStepTriggerAttempt(Entity<GiantMouseTrapComponent> ent, ref StepTriggerAttemptEvent args)
    {
        args.Cancelled = !HasComp<AlwaysTriggerMousetrapComponent>(args.Tripper);
    }

    private void BeforeDamageOnTrigger(Entity<GiantMouseTrapComponent> ent, ref BeforeDamageOnTriggerEvent args)
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

        if (TryComp<ItemToggleComponent>(ent, out var toggle))
            _itemToggle.Toggle((ent.Owner, toggle), args.User);

        args.Handled = true;
    }
}
