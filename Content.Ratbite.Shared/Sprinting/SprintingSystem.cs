using System.Numerics;
using Content.Goobstation.Common.Movement;
using Content.Shared._EinsteinEngines.Flight.Events;
using Content.Shared.Bed.Sleep;
using Content.Shared.Buckle.Components;
using Content.Shared.CombatMode;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage.Events;
using Content.Shared.Damage.Systems;
using Content.Shared.Gravity;
using Content.Shared.Mech.Components;
using Content.Shared.Mech.EntitySystems;
using Content.Shared.Mobs;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Content.Shared.Popups;
using Content.Shared.Standing;
using Content.Shared.Stunnable;
using Content.Trauma.Common.Input;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Input;
using Robust.Shared.Input.Binding;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Ratbite.Shared.Sprinting;

public abstract partial class SharedSprintingSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private SharedStaminaSystem _staminaSystem = default!;
    [Dependency] private MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedGravitySystem _gravity = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private StandingStateSystem _standing = default!;
    [Dependency] private DamageableSystem _damageable = default!;
    [Dependency] private SharedMoverController _moverController = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<SprinterComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshSpeed);
        CommandBinds.Builder
            .Bind(TraumaKeyFunctions.Sprint, new SprintInputCmdHandler(this))
            .Register<SharedSprintingSystem>();
        SubscribeLocalEvent<SprinterComponent, SprintToggleEvent>(OnSprintToggle);
        SubscribeLocalEvent<SprinterComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
        SubscribeLocalEvent<SprinterComponent, BeforeStaminaDamageEvent>(OnBeforeStaminaDamage);
        SubscribeLocalEvent<SprinterComponent, SleepStateChangedEvent>(OnSleep);
        SubscribeLocalEvent<SprinterComponent, FlightEvent>(OnFlight);
        SubscribeLocalEvent<SprinterComponent, MechEntryEvent>(OnMechEntry);
        SubscribeLocalEvent<SprinterComponent, ToggleWalkEvent>(OnToggleWalk);
        SubscribeLocalEvent<SprinterComponent, KnockedDownEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<SprinterComponent, StunnedEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<SprinterComponent, DownedEvent>(OnSprintDisablingEvent);
        SubscribeLocalEvent<CuffableComponent, SprintAttemptEvent>(OnCuffableSprintAttempt);
        SubscribeLocalEvent<MechPilotComponent, SprintAttemptEvent>(OnMechPilotSprintAttempt);
        SubscribeLocalEvent<StandingStateComponent, SprintAttemptEvent>(OnStandingStateSprintAttempt);
        SubscribeLocalEvent<BuckleComponent, SprintAttemptEvent>(OnBuckleSprintAttempt);
        SubscribeLocalEvent<SprinterComponent, DisarmedEvent>(OnDisarm);
    }

    private sealed class SprintInputCmdHandler(SharedSprintingSystem system) : InputCmdHandler
    {
        public override bool HandleCmdMessage(IEntityManager entManager, ICommonSession? session, IFullInputCmdMessage message)
        {
            if (session?.AttachedEntity == null)
                return false;

            system.HandleSprintInput(session, message);
            return false;
        }
    }

    /*
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // We dont add it to the EQE since the comp might get added as this runs.
        var query = EntityQueryEnumerator<SprinterComponent, StaminaModifierComponent>();
        while (query.MoveNext(out var uid, out var sprinterComp, out var staminaComp))
        {
            if (!sprinterComp.IsSprinting
                || !sprinterComp.ScaleWithStamina
                || staminaComp.Modifier <= 1f)
                continue;

            _staminaSystem.ModifyStaminaDrain(uid,
                sprinterComp.StaminaDrainKey,
                sprinterComp.StaminaDrainRate * staminaComp.Modifier * sprinterComp.StaminaDrainMultiplier);
        }
    }
    */

    private void OnRefreshSpeed(Entity<SprinterComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        args.ModifySpeed(ent.Comp.SprintSpeedMultiplier);
    }

    private void HandleSprintInput(ICommonSession? session, IFullInputCmdMessage message)
    {
        if (session?.AttachedEntity == null
            || !TryComp<SprinterComponent>(session.AttachedEntity, out var sprinterComponent)
            || !TryComp<InputMoverComponent>(session.AttachedEntity, out var inputMoverComponent)
            || !sprinterComponent.IsSprinting
            // We check this instead of physics so that we can gatekeep sprinting to only work when you are moving intentionally, and not walking.
            && _moverController.GetVelocityInput(inputMoverComponent).Sprinting == Vector2.Zero)
            return;

        if (!sprinterComponent.CanSprint)
        {
            if (message.State == BoundKeyState.Down) // Without this check the message triggers when holding and releasing.
                _popupSystem.PopupClient(Loc.GetString("sprint-disabled"), session.AttachedEntity.Value, session.AttachedEntity.Value, PopupType.Medium);

            return;
        }

        RaiseLocalEvent(session.AttachedEntity.Value, new SprintToggleEvent(!sprinterComponent.IsSprinting && message.State == BoundKeyState.Down));
    }

    private void OnSprintToggle(Entity<SprinterComponent> ent, ref SprintToggleEvent args) =>
        ToggleSprint(ent, args.IsSprinting);

    public void ToggleSprint(Entity<SprinterComponent> ent, bool newSprintState, bool gracefulStop = true)
    {
        // Breaking these into two separate if's for better readability
        if (newSprintState == ent.Comp.IsSprinting)
            return;

        if (newSprintState
            && (!CanSprint(ent)
            || _timing.CurTime - ent.Comp.LastSprint < ent.Comp.TimeBetweenSprints))
            return;

        ent.Comp.LastSprint = _timing.CurTime;
        ent.Comp.IsSprinting = newSprintState;

        if (newSprintState)
        {
            RaiseLocalEvent(ent.Owner, new SprintStartEvent());
            _audio.PlayPredicted(ent.Comp.SprintStartupSound, ent.Owner, ent.Owner);
        }

        if (!gracefulStop)
            _damageable.TryChangeDamage(ent.Owner, ent.Comp.SprintDamageSpecifier);

        _movementSpeed.RefreshMovementSpeedModifiers(ent.Owner);
        _staminaSystem.ToggleStaminaDrain(ent.Owner, ent.Comp.StaminaDrainRate, newSprintState, true, ent.Comp.StaminaDrainKey, ent.Owner);
        Dirty(ent);
    }

    private bool CanSprint(Entity<SprinterComponent> ent)
    {
        // Awaiting on a wizden PR that refactors gravity from whatever the fuck this is.
        if (_gravity.IsWeightless(ent.Owner))
        {
            _popupSystem.PopupClient(Loc.GetString("no-sprint-while-weightless"), ent.Owner, ent.Owner, PopupType.Medium);
            return false;
        }

        var ev = new SprintAttemptEvent();
        RaiseLocalEvent(ent, ref ev);

        return !ev.Cancelled;
    }

    private void OnCuffableSprintAttempt(Entity<CuffableComponent> ent, ref SprintAttemptEvent args)
    {
        if (ent.Comp.CanStillInteract)
            return;

        _popupSystem.PopupClient(Loc.GetString("no-sprint-while-restrained"), ent.Owner, ent.Owner, PopupType.Medium);
        args.Cancel();
    }

    private void OnStandingStateSprintAttempt(Entity<StandingStateComponent> ent, ref SprintAttemptEvent args)
    {
        if (!_standing.IsDown(ent.Owner))
            return;

        _popupSystem.PopupClient(Loc.GetString("no-sprint-while-lying"), ent.Owner, ent.Owner, PopupType.Medium);
        args.Cancel();
    }

    private void OnBuckleSprintAttempt(Entity<BuckleComponent> ent, ref SprintAttemptEvent args)
    {
        if (ent.Comp.BuckledTo == null
            || !TryComp<SprinterComponent>(ent.Comp.BuckledTo, out var sprinterComponent)
            || sprinterComponent.IsSprinting)
            return;

        args.Cancel();
    }

    private void OnMechPilotSprintAttempt(Entity<MechPilotComponent> ent, ref SprintAttemptEvent args)
    {
        if (!TryComp<SprinterComponent>(ent.Comp.Mech, out var sprinterComponent)
            || sprinterComponent.IsSprinting)
            return;

        args.Cancel();
    }

    private void OnBeforeStaminaDamage(Entity<SprinterComponent> ent, ref BeforeStaminaDamageEvent args)
    {
        if (!ent.Comp.IsSprinting
            || args.Value > 0)
            return;

        args.Value *= ent.Comp.StaminaRegenMultiplier;
    }

    private void OnMobStateChangedEvent(Entity<SprinterComponent> ent, ref MobStateChangedEvent args)
    {
        if (!ent.Comp.IsSprinting || args.NewMobState is MobState.Critical or MobState.Dead)
            return;

        ToggleSprint(ent, false, gracefulStop: false);
    }

    private void OnSleep(Entity<SprinterComponent> ent, ref SleepStateChangedEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        ToggleSprint(ent, false, gracefulStop: false);
    }

    private void OnFlight(Entity<SprinterComponent> ent, ref FlightEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        ToggleSprint(ent, false);
    }

    private void OnMechEntry(Entity<SprinterComponent> ent, ref MechEntryEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        ToggleSprint(ent, false);
    }

    private void OnToggleWalk(Entity<SprinterComponent> ent, ref ToggleWalkEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        ToggleSprint(ent, false);
    }

    private void OnSprintDisablingEvent<T>(Entity<SprinterComponent> ent, ref T args) where T : notnull
    {
        if (!ent.Comp.IsSprinting)
            return;

        ToggleSprint(ent, false, gracefulStop: false);
    }

    private void OnDisarm(Entity<SprinterComponent> ent, ref DisarmedEvent args)
    {
        if (!ent.Comp.IsSprinting)
            return;

        _staminaSystem.TakeStaminaDamage(ent.Owner, ent.Comp.StaminaPenaltyOnShove);
        ToggleSprint(ent, false, gracefulStop: true);
    }

}
