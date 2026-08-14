using Content.Shared._BRatbite.Revolutionary;
using Content.Shared.Armor;
using Content.Shared.Body.Systems;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Content.Shared.Mindshield.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._BRatbite.Armor;

public sealed partial class MindshieldArmorSystem : EntitySystem
{
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifierSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MindshieldArmorComponent, InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeed);
        SubscribeLocalEvent<MindshieldArmorComponent, ArmorExamineEvent>(OnArmorExamine);
        SubscribeLocalEvent<MovementSpeedModifierComponent, MindShieldChangedEvent>(OnMindshieldChanged);
    }

    private void OnMindshieldChanged(Entity<MovementSpeedModifierComponent> ent, ref MindShieldChangedEvent args)
    {
        if (TerminatingOrDeleted(ent)) return;
        _movementSpeedModifierSystem.RefreshMovementSpeedModifiers(ent);
    }

    private void OnRefreshMovementSpeed(Entity<MindshieldArmorComponent> ent, ref InventoryRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (HasComp<MindShieldComponent>(args.Args.User) || HasComp<FakeMindShieldComponent>(args.Args.User)) return;
        args.Args.ModifySpeed(ent.Comp.Slowdown);
    }

    private void OnArmorExamine(Entity<MindshieldArmorComponent> ent, ref ArmorExamineEvent args)
    {
        var msg = args.Msg;
        if (msg is null) return; // Sometimes msg was null during testing, I
                         // think it was because I was hot reloading
                         // the yaml, but I'm going to add this just
                         // in case
        msg.PushNewline();
        msg.AddMarkupOrThrow(Loc.GetString("mindshield-armor-slowdown", [("value", MathF.Round((1f - ent.Comp.Slowdown) * 100f))]));
    }
}
