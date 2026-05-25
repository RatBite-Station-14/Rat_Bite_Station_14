// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Weapons.Melee;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Traits;

public sealed class PctTrainingSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly UnarmedCombatSkillSystem _unarmedCombat = default!;

    private readonly List<PendingPctKnockout> _pendingKnockouts = new();
    private readonly List<PendingPctStars> _pendingStars = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PctTrainingComponent, AttackAttemptEvent>(OnAttackAttempt);
        SubscribeLocalEvent<PctTrainingComponent, GetUserMeleeDamageEvent>(OnGetUserMeleeDamage);
        SubscribeLocalEvent<PctTrainingComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<PctTrainingComponent, MeleeHitEvent>(OnMeleeHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_net.IsServer)
            return;

        for (var i = _pendingStars.Count - 1; i >= 0; i--)
        {
            var pending = _pendingStars[i];

            if (Deleted(pending.User) || pending.EndTime <= _timing.CurTime)
            {
                _pendingStars.RemoveAt(i);

                if (!Deleted(pending.User))
                    _appearance.SetData(pending.User, SharedStunSystem.StunVisuals.SeeingStars, false);
            }
        }

        for (var i = _pendingKnockouts.Count - 1; i >= 0; i--)
        {
            var pending = _pendingKnockouts[i];
            _pendingKnockouts.RemoveAt(i);

            if (Deleted(pending.User) || Deleted(pending.Target))
                continue;

            if (!TryComp<MobStateComponent>(pending.Target, out var mobState))
                continue;

            if (mobState.CurrentState <= pending.OldState ||
                mobState.CurrentState is not (MobState.Critical or MobState.Dead))
                continue;

            RaiseLocalEvent(pending.User,
                new PctTrainingKnockoutEvent(
                    pending.User,
                    pending.Target,
                    pending.Direction,
                    pending.ThrowDistance,
                    pending.ThrowSpeed));
        }
    }

    private void OnAttackAttempt(Entity<PctTrainingComponent> ent, ref AttackAttemptEvent args)
    {
        if (_unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (ent.Comp.BlockedUntil <= _timing.CurTime)
            return;

        args.Cancel();
    }

    private void OnGetUserMeleeDamage(Entity<PctTrainingComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (_unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner)
            return;

        var bonus = new DamageSpecifier();
        bonus.DamageDict.Add("Blunt", FixedPoint2.New(ent.Comp.BluntBonus));
        args.Damage += bonus;
    }

    private void OnGetMeleeAttackRate(Entity<PctTrainingComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (_unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner || ent.Comp.Combo <= 0)
            return;

        args.Rate += ent.Comp.Combo * ent.Comp.ComboAttackRateBonus;
    }

    private void OnMeleeHit(Entity<PctTrainingComponent> ent, ref MeleeHitEvent args)
    {
        if (_unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
        {
            ClearPctState(ent);
            return;
        }

        if (args.Weapon != ent.Owner || !args.IsHit)
            return;

        if (IsCleanMobHit(args))
        {
            QueueKnockoutChecks(ent, args);

            ent.Comp.Combo = Math.Min(ent.Comp.MaxCombo, ent.Comp.Combo + 1);
            return;
        }

        ent.Comp.Combo = 0;
        ent.Comp.BlockedUntil = _timing.CurTime + ent.Comp.FumbleCooldown;
        if (TryComp<MeleeWeaponComponent>(args.Weapon, out var weapon) && weapon.NextAttack < ent.Comp.BlockedUntil)
        {
            weapon.NextAttack = ent.Comp.BlockedUntil;
            DirtyField(args.Weapon, weapon, nameof(MeleeWeaponComponent.NextAttack));
        }

        _alerts.ShowAlert(ent.Owner,
            ent.Comp.FumbleAlert,
            cooldown: (_timing.CurTime, ent.Comp.BlockedUntil),
            autoRemove: true);

        if (!_net.IsServer)
            return;

        _stun.TrySeeingStars(ent.Owner);
        for (var i = _pendingStars.Count - 1; i >= 0; i--)
        {
            if (_pendingStars[i].User == ent.Owner)
                _pendingStars.RemoveAt(i);
        }

        _pendingStars.Add(new PendingPctStars(ent.Owner, _timing.CurTime + ent.Comp.FumbleStarsDuration));
        RaiseLocalEvent(ent.Owner, new PctTrainingFumbleEvent(ent.Comp.FumbleSpeech));
    }

    private void QueueKnockoutChecks(Entity<PctTrainingComponent> ent, MeleeHitEvent args)
    {
        if (!_net.IsServer)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<MobStateComponent>(target, out var mobState))
                continue;

            var direction = GetThrowDirection(ent.Owner, target);
            _pendingKnockouts.Add(new PendingPctKnockout(
                ent.Owner,
                target,
                mobState.CurrentState,
                direction,
                ent.Comp.KnockoutThrowDistance,
                ent.Comp.KnockoutThrowSpeed));
        }
    }

    private void ClearPctState(Entity<PctTrainingComponent> ent)
    {
        if (ent.Comp.Combo == 0 && ent.Comp.BlockedUntil <= _timing.CurTime)
            return;

        ent.Comp.Combo = 0;
        ent.Comp.BlockedUntil = TimeSpan.Zero;
        _alerts.ClearAlert(ent.Owner, ent.Comp.FumbleAlert);

        if (TryComp<MeleeWeaponComponent>(ent.Owner, out var weapon) && weapon.NextAttack > _timing.CurTime)
        {
            weapon.NextAttack = _timing.CurTime;
            DirtyField(ent.Owner, weapon, nameof(MeleeWeaponComponent.NextAttack));
        }
    }

    private Vector2 GetThrowDirection(EntityUid user, EntityUid target)
    {
        var userPos = _transform.GetMapCoordinates(user).Position;
        var targetPos = _transform.GetMapCoordinates(target).Position;
        var direction = targetPos - userPos;

        return direction == Vector2.Zero ? Vector2.UnitX : direction.Normalized();
    }

    private bool IsCleanMobHit(MeleeHitEvent args)
    {
        if (args.HitEntities.Count == 0)
            return false;

        foreach (var hit in args.HitEntities)
        {
            if (!HasComp<MobStateComponent>(hit))
                return false;
        }

        return true;
    }

    private readonly record struct PendingPctKnockout(
        EntityUid User,
        EntityUid Target,
        MobState OldState,
        Vector2 Direction,
        float ThrowDistance,
        float ThrowSpeed);

    private readonly record struct PendingPctStars(EntityUid User, TimeSpan EndTime);
}

public sealed class PctTrainingKnockoutEvent(
    EntityUid user,
    EntityUid target,
    Vector2 direction,
    float throwDistance,
    float throwSpeed) : EntityEventArgs
{
    public readonly EntityUid User = user;
    public readonly EntityUid Target = target;
    public readonly Vector2 Direction = direction;
    public readonly float ThrowDistance = throwDistance;
    public readonly float ThrowSpeed = throwSpeed;
}

public sealed class PctTrainingFumbleEvent(LocId speech) : EntityEventArgs
{
    public readonly LocId Speech = speech;
}
