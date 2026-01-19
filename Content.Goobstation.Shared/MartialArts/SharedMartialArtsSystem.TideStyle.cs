// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Common.MartialArts;
using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared._Shitmed.Targeting;
using Content.Shared.Body.Part;
using Content.Shared.Damage.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Components;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Movement.Systems;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Content.Goobstation.Shared.MartialArts.Events;
using Content.Shared.Body.Components;
using Content.Shared.Body.Systems;
using Content.Shared.Interaction.Events;
using Content.Shared.Examine;
using Content.Shared.IdentityManagement;
using Content.Shared.CombatMode;
using Content.Shared.Weapons.Melee;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Robust.Shared.Random;
using Robust.Shared.Map;
using Robust.Shared.Player;

namespace Content.Goobstation.Shared.MartialArts;

public partial class SharedMartialArtsSystem
{
    [Dependency] private readonly WoundSystem _wounds = default!;
    [Dependency] private readonly SharedBodySystem _body = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;

    private void InitializeTideStyle()
    {
        SubscribeLocalEvent<TideStyleComponent, MeleeHitEvent>(OnTideStyleMeleeHit);
        SubscribeLocalEvent<TideStyleComponent, DisarmedEvent>(OnTideStyleShove);
        SubscribeLocalEvent<TideStyleComponent, ComponentShutdown>(OnTideStyleShutdown);
        SubscribeLocalEvent<TideStyleComponent, TideStyleAbilityEvent>(OnTideStyleAbility);
        SubscribeLocalEvent<TideStyleComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovespeed);
        SubscribeLocalEvent<TideStyleComponent, TideStyleAbilityDoAfterEvent>(OnAbilityDoAfterComplete);

        SubscribeNetworkEvent<TideStyleBodyPartSelectedEvent>(OnBodyPartSelected);
        SubscribeNetworkEvent<TideStyleAbilityCancelEvent>(OnAbilityCancelled);

        SubscribeLocalEvent<GrantTideStyleComponent, UseInHandEvent>(OnGrantTideStyleUse);
        SubscribeLocalEvent<GrantTideStyleComponent, ExaminedEvent>(OnGrantTideStyleExamine);
    }

    private void OnAbilityCancelled(TideStyleAbilityCancelEvent args, EntitySessionEventArgs session)
    {
        if (!TryGetEntity(args.User, out var user))
            return;

        if (!TryComp<TideStyleComponent>(user, out var comp))
            return;

        if (!comp.PendingBodyPartSelection)
            return;

        ClearAbility((user.Value, comp));
        _popupSystem.PopupClient(Loc.GetString("tidestyle-ability-cancelled"), user.Value, user.Value);
    }

    private void OnRefreshMovespeed(Entity<TideStyleComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (ent.Comp.DoAfterId != null || ent.Comp.AbilityReady)
        {
            args.ModifySpeed(ent.Comp.MovementSpeedModifier, ent.Comp.MovementSpeedModifier);
        }
    }

    private void OnTideStyleShutdown(Entity<TideStyleComponent> ent, ref ComponentShutdown args)
    {
        foreach (var action in ent.Comp.TideStyleAbilityEntities)
            _actions.RemoveAction(action);

        if (ent.Comp.DoAfterId != null)
            _doAfter.Cancel(ent, ent.Comp.DoAfterId.Value);
    }

    private void OnGrantTideStyleUse(Entity<GrantTideStyleComponent> ent, ref UseInHandEvent args)
    {
        if (!_netManager.IsServer)
            return;

        if (ent.Comp.Used)
        {
            _popupSystem.PopupEntity(Loc.GetString("tidestyle-fail-used", ("manual", Identity.Entity(ent, EntityManager))),
                args.User, args.User);
            return;
        }

        if (HasComp<ChangelingIdentityComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-changeling"), args.User, args.User);
            return;
        }

        if (HasComp<MartialArtsKnowledgeComponent>(args.User))
        {
            _popupSystem.PopupEntity(Loc.GetString("cqc-fail-knowanother"), args.User, args.User);
            return;
        }

        if (!TryGrantTideStyle(args.User))
            return;

        _popupSystem.PopupEntity(Loc.GetString("tidestyle-success-learned"), args.User, args.User);
        ent.Comp.Used = true;
    }

    private bool TryGrantTideStyle(EntityUid user)
    {
        if (MetaData(user).EntityLifeStage >= EntityLifeStage.Terminating)
            return false;

        var tideStyleComp = EnsureComp<TideStyleComponent>(user);

        var ripAction = _actions.AddAction(user, "ActionTideRip");
        if (ripAction != null)
            tideStyleComp.TideStyleAbilityEntities.Add(ripAction.Value);

        var stompAction = _actions.AddAction(user, "ActionTideStomp");
        if (stompAction != null)
            tideStyleComp.TideStyleAbilityEntities.Add(stompAction.Value);

        var pushAction = _actions.AddAction(user, "ActionTidePush");
        if (pushAction != null)
            tideStyleComp.TideStyleAbilityEntities.Add(pushAction.Value);

        var knowledge = EnsureComp<MartialArtsKnowledgeComponent>(user);
        knowledge.MartialArtsForm = MartialArtsForms.TideStyle;
        knowledge.StartingStage = GrabStage.Soft;
        knowledge.Blocked = false;

        EnsureComp<CanPerformComboComponent>(user);
        EnsureComp<PullerComponent>(user);

        if (TryComp<MeleeWeaponComponent>(user, out var meleeWeapon))
        {
            if (meleeWeapon.Damage.DamageDict.Count != 0)
            {
                knowledge.OriginalFistDamage = meleeWeapon.Damage.DamageDict.Values.ElementAt(0).Float();
                knowledge.OriginalFistDamageType = meleeWeapon.Damage.DamageDict.Keys.ElementAt(0);
            }
        }

        return true;
    }

    private void OnGrantTideStyleExamine(Entity<GrantTideStyleComponent> ent, ref ExaminedEvent args)
    {
        if (ent.Comp.Used)
            args.PushMarkup(Loc.GetString("tidestyle-manual-used", ("manual", Identity.Entity(ent, EntityManager))));
    }

    private void OnTideStyleAbility(Entity<TideStyleComponent> ent, ref TideStyleAbilityEvent args)
    {
        var actionEnt = args.Action.Owner;
        if (!TryComp<TideStyleAbilityComponent>(actionEnt, out var abilityComp))
            return;

        if (ent.Comp.DoAfterId != null)
        {
            _doAfter.Cancel(ent, ent.Comp.DoAfterId.Value);
            ClearAbility(ent);
        }

        if (abilityComp.Configuration == TideStyleAbility.Push)
        {
            var curTime = _timing.CurTime;
            // Apply consecutive hits cooldown reduction
            var cooldownReduction = Math.Min(ent.Comp.ConsecutiveHits * ent.Comp.HitCooldownReductionPerHit, ent.Comp.MaxHitCooldownReduction);
            var actualCooldown = Math.Max(ent.Comp.PushCooldown - cooldownReduction, 0.5f);

            if (curTime < ent.Comp.LastPushTime + TimeSpan.FromSeconds(actualCooldown))
            {
                var remaining = (ent.Comp.LastPushTime + TimeSpan.FromSeconds(actualCooldown) - curTime).TotalSeconds;
                _popupSystem.PopupClient(Loc.GetString("tide-push-cooldown", ("seconds", Math.Ceiling(remaining))), ent, ent);
                return;
            }

            _popupSystem.PopupClient(Loc.GetString("tide-push-ready"), ent, ent);
            ent.Comp.SelectedAbility = TideStyleAbility.Push;
            ent.Comp.AbilityReady = true;
            Dirty(ent);
            return;
        }

        ent.Comp.SelectedAbility = abilityComp.Configuration;
        ent.Comp.PendingBodyPartSelection = true;
        ent.Comp.AbilityReady = false;
        Dirty(ent);
    }

    private void OnBodyPartSelected(TideStyleBodyPartSelectedEvent args, EntitySessionEventArgs session)
    {
        if (!TryGetEntity(args.User, out var user))
            return;

        if (!TryComp<TideStyleComponent>(user, out var comp))
            return;

        if (!comp.PendingBodyPartSelection)
            return;

        comp.SelectedBodyPart = args.BodyPart;
        comp.PendingBodyPartSelection = false;
        Dirty(user.Value, comp);

        EntityUid? target = null;
        if (TryComp<CanPerformComboComponent>(user, out var combo))
            target = combo.CurrentTarget;

        float baseDelay;
        float variation;

        if (comp.SelectedAbility == TideStyleAbility.Rip)
        {
            baseDelay = comp.RipDelayBase;
            variation = comp.RipDelayVariation;
        }
        else
        {
            baseDelay = comp.StompDelayBase;
            variation = comp.StompDelayVariation;
        }

        // Apply hit reduction bonus
        var hitReduction = Math.Min(comp.ConsecutiveHits * comp.HitDelayReductionPerHit, comp.MaxHitDelayReduction);
        var multiplier = CalculateDelayMultiplier(target, args.BodyPart);
        var actualDelay = (_random.NextFloat(baseDelay - variation, baseDelay + variation) - hitReduction) * multiplier;
        actualDelay = Math.Max(actualDelay, 0.5f);

        var abilityName = comp.SelectedAbility == TideStyleAbility.Rip ? "Rip" : "Stomp";
        _popupSystem.PopupClient(Loc.GetString("tide-style-ready", ("action", abilityName)), user.Value, user.Value);

        var doAfterArgs = new DoAfterArgs(EntityManager, user.Value, TimeSpan.FromSeconds(actualDelay), new TideStyleAbilityDoAfterEvent(), user.Value)
        {
            BreakOnMove = false,
            BreakOnDamage = false,
            BlockDuplicate = true,
            CancelDuplicate = true,
        };

        if (_doAfter.TryStartDoAfter(doAfterArgs, out var doAfterId))
        {
            comp.DoAfterId = doAfterId.Value.Index;
            Dirty(user.Value, comp);
            _movement.RefreshMovementSpeedModifiers(user.Value);
        }
    }

    private float CalculateDelayMultiplier(EntityUid? target, TargetBodyPart bodyPart)
    {
        float multiplier = 1.0f;

        if (bodyPart == TargetBodyPart.LeftHand || bodyPart == TargetBodyPart.RightHand ||
            bodyPart == TargetBodyPart.LeftFoot || bodyPart == TargetBodyPart.RightFoot)
        {
            multiplier *= 0.6f;
        }
        else if (bodyPart == TargetBodyPart.Head)
        {
            multiplier *= 1.5f;
        }

        if (target == null || !TryComp<BodyComponent>(target.Value, out var body))
            return multiplier;

        switch (bodyPart)
        {
            case TargetBodyPart.LeftArm:
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Hand, symmetry: BodyPartSymmetry.Left).Any())
                    multiplier *= 1.3f;
                break;
            case TargetBodyPart.RightArm:
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Hand, symmetry: BodyPartSymmetry.Right).Any())
                    multiplier *= 1.3f;
                break;
            case TargetBodyPart.LeftLeg:
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Foot, symmetry: BodyPartSymmetry.Left).Any())
                    multiplier *= 1.3f;
                break;
            case TargetBodyPart.RightLeg:
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Foot, symmetry: BodyPartSymmetry.Right).Any())
                    multiplier *= 1.3f;
                break;
            case TargetBodyPart.Groin:
                var connectedCount = 0;
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Leg, symmetry: BodyPartSymmetry.Left).Any())
                    connectedCount++;
                if (_body.GetBodyChildrenOfType(target.Value, BodyPartType.Leg, symmetry: BodyPartSymmetry.Right).Any())
                    connectedCount++;
                multiplier *= 1.0f + (connectedCount * 1.5f);
                break;
        }

        return multiplier;
    }

    private void OnAbilityDoAfterComplete(Entity<TideStyleComponent> ent, ref TideStyleAbilityDoAfterEvent args)
    {
        ent.Comp.DoAfterId = null;

        if (args.Cancelled)
        {
            ClearAbility(ent);
            return;
        }

        ent.Comp.AbilityReady = true;
        ent.Comp.DoAfterCompletionTime = _timing.CurTime;
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void OnTideStyleMeleeHit(Entity<TideStyleComponent> ent, ref MeleeHitEvent args)
    {
        if (args.HitEntities.Count <= 0)
            return;

        var target = args.HitEntities[0];
        var curTime = _timing.CurTime;

        if (curTime > ent.Comp.LastHitTime + TimeSpan.FromSeconds(ent.Comp.HitDecayTime))
        {
            ent.Comp.ConsecutiveHits = 1;
        }
        else
        {
            ent.Comp.ConsecutiveHits++;
        }

        ent.Comp.LastHitTime = curTime;
        Dirty(ent);

        if (ent.Comp.AbilityReady && ent.Comp.SelectedAbility != null)
        {
            if (ent.Comp.SelectedAbility != TideStyleAbility.Push)
            {
                if (!IsWithinAttackWindow(ent))
                {
                    _popupSystem.PopupClient(Loc.GetString("tidestyle-ability-mistimed"), ent, ent);
                    ClearAbility(ent);
                    return;
                }
            }

            var targetPart = ent.Comp.SelectedBodyPart ?? TargetBodyPart.Chest;
            var isDown = TryComp<RequireProjectileTargetComponent>(target, out var downComp) && downComp.Active;

            switch (ent.Comp.SelectedAbility.Value)
            {
                case TideStyleAbility.Rip:
                    HandleRipAbility(ent, target, targetPart, isDown);
                    break;
                case TideStyleAbility.Stomp:
                    HandleStompAbility(ent, target, targetPart, isDown);
                    break;
                case TideStyleAbility.Push:
                    HandlePushAbility(ent, target);
                    ent.Comp.LastPushTime = curTime;
                    break;
            }

            ClearAbility(ent);
            return;
        }

        if (curTime < ent.Comp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
            return;

        ent.Comp.LastSoundTime = curTime;
        DoDamage(ent.Owner, target, "Blunt", (int)ent.Comp.PunchDamage, out _);

        if (_random.Prob(ent.Comp.BoneBrokenChance))
        {
            if (TryComp<TargetingComponent>(ent, out var targeting))
                TryBreakBoneAtTarget(target, targeting.Target);
        }
    }

    private bool IsWithinAttackWindow(Entity<TideStyleComponent> ent)
    {
        if (ent.Comp.DoAfterCompletionTime == null)
            return false;

        var timeSinceCompletion = (_timing.CurTime - ent.Comp.DoAfterCompletionTime.Value).TotalSeconds;
        return timeSinceCompletion >= ent.Comp.AttackWindowStart &&
               timeSinceCompletion <= ent.Comp.AttackWindowEnd;
    }

    private void TryBreakBoneAtTarget(EntityUid target, TargetBodyPart targetPart)
    {
        if (!TryComp<BodyComponent>(target, out var body))
            return;

        var bodyPartType = GetBodyPartFromTarget(targetPart);
        var symmetry = GetSymmetryFromTarget(targetPart);

        var part = _body.GetBodyChildrenOfType(target, bodyPartType, symmetry: symmetry).FirstOrDefault();
        if (part == default || !TryComp<WoundableComponent>(part.Id, out var woundable))
            return;

        var boneEntity = woundable.Bone.ContainedEntities.FirstOrDefault();
        if (boneEntity != default(EntityUid))
        {
            _trauma.ApplyDamageToBone(boneEntity, 50);
        }
    }

    private void HandlePushAbility(EntityUid user, EntityUid target)
    {
        var tideComp = Comp<TideStyleComponent>(user);

        var curTime = _timing.CurTime;
        if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Effects/hit_kick.ogg"), target);
            tideComp.LastSoundTime = curTime;
        }

        DoDamage(user, target, "Blunt", (int)tideComp.PushDamage, out _);

        var mapPos = _transform.GetMapCoordinates(user).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;

        if (dir.Length() > 0)
            dir *= tideComp.PushKnockbackDistance / dir.Length();

        _grabThrowing.Throw(target, user, dir, 5f);
        _stun.TryKnockdown(target, TimeSpan.FromSeconds(tideComp.PushStunTime), true);

        if (TryComp<PullableComponent>(target, out var pullable))
            _pulling.TryStopPull(target, pullable, user, true);

        _popupSystem.PopupClient(Loc.GetString("tide-push-success"), user, user);
    }

    private void ClearAbility(Entity<TideStyleComponent> ent)
    {
        ent.Comp.SelectedAbility = null;
        ent.Comp.AbilityReady = false;
        ent.Comp.DoAfterId = null;
        ent.Comp.DoAfterCompletionTime = null;
        ent.Comp.SelectedBodyPart = null;
        ent.Comp.PendingBodyPartSelection = false;
        ent.Comp.SecondDoAfter = false;
        Dirty(ent);
        _movement.RefreshMovementSpeedModifiers(ent);
    }

    private void HandleStompAbility(EntityUid user, EntityUid target, TargetBodyPart targetPart, bool isDown)
    {
        if (!isDown)
        {
            _popupSystem.PopupClient(Loc.GetString("tide-stomp-not-downed"), user, user);
            return;
        }

        if (!TryComp<BodyComponent>(target, out var body))
            return;

        // Base allowed parts
        var allowedParts = new[] {
            TargetBodyPart.Head, TargetBodyPart.LeftArm, TargetBodyPart.RightArm,
            TargetBodyPart.LeftHand, TargetBodyPart.RightHand,
            TargetBodyPart.LeftLeg, TargetBodyPart.RightLeg,
            TargetBodyPart.LeftFoot, TargetBodyPart.RightFoot
        };

        allowedParts = allowedParts.Concat(new[] {
            TargetBodyPart.Groin, TargetBodyPart.Chest
        }).ToArray();

        if (!allowedParts.Contains(targetPart))
        {
            _popupSystem.PopupClient(Loc.GetString("tide-stomp-invalid-part"), user, user);
            return;
        }

        var actualTargetPart = targetPart;
        if (targetPart == TargetBodyPart.Groin || targetPart == TargetBodyPart.Chest)
        {
            var groinPart = _body.GetBodyChildrenOfType(target, BodyPartType.Groin).FirstOrDefault();
            var chestPart = _body.GetBodyChildrenOfType(target, BodyPartType.Chest).FirstOrDefault();

            if (groinPart != default)
            {
                actualTargetPart = TargetBodyPart.Groin;
            }
            else if (chestPart != default)
            {
                actualTargetPart = TargetBodyPart.Chest;
            }
            else
            {
                var curTime = _timing.CurTime;
                var tideComp = Comp<TideStyleComponent>(user);
                if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
                {
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Organs/OrganDestroyed2.ogg"), target);
                    tideComp.LastSoundTime = curTime;
                }
                return;
            }
        }

        var bodyPartType = GetBodyPartFromTarget(actualTargetPart);
        var symmetry = GetSymmetryFromTarget(actualTargetPart);

        var part = _body.GetBodyChildrenOfType(target, bodyPartType, symmetry: symmetry).FirstOrDefault();
        if (part == default || !TryComp<WoundableComponent>(part.Id, out var woundable))
        {
            var curTime = _timing.CurTime;
            var tideComp = Comp<TideStyleComponent>(user);
            if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Organs/OrganDestroyed2.ogg"), target);
                tideComp.LastSoundTime = curTime;
            }
            return;
        }

        if (!_body.TryGetParentBodyPart(part.Id, out var parentUid, out _) || parentUid == null)
        {
            var curTime = _timing.CurTime;
            var tideComp = Comp<TideStyleComponent>(user);

            if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Bone/BoneGone5.ogg"), target);
                tideComp.LastSoundTime = curTime;
            }

            _body.GibBody(target, gibOrgans: true);
            _popupSystem.PopupClient(Loc.GetString("tide-stomp-success"), user, user);
            return;
        }


        if (TryComp<WoundableComponent>(parentUid.Value, out var parentWoundable))
        {
            if (_wounds.TryInduceWound(parentUid.Value, "Blunt", 5f, out var bleedWound))
            {
                if (TryComp<BleedInflicterComponent>(bleedWound.Value.Owner, out var bleedComp))
                {
                    bleedComp.IsBleeding = true;
                    bleedComp.BleedingAmountRaw = 10f;
                    bleedComp.ScalingLimit += 10;
                    Dirty(bleedWound.Value.Owner, bleedComp);
                }
            }
        }

        _wounds.DestroyWoundable(parentUid.Value, part.Id, woundable);

        var curTime2 = _timing.CurTime;
        var tideComp2 = Comp<TideStyleComponent>(user);
        if (curTime2 >= tideComp2.LastSoundTime + TimeSpan.FromSeconds(0.1f))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Bone/BoneGone5.ogg"), target);
            tideComp2.LastSoundTime = curTime2;
        }

        _popupSystem.PopupClient(Loc.GetString("tide-stomp-success"), user, user);
    }

    private void HandleRipAbility(EntityUid user, EntityUid target, TargetBodyPart targetPart, bool isDown)
    {
        if (!TryComp<BodyComponent>(target, out var body))
            return;

        // Base allowed parts
        var allowedParts = new[] {
            TargetBodyPart.LeftHand, TargetBodyPart.RightHand,
            TargetBodyPart.LeftArm, TargetBodyPart.RightArm,
            TargetBodyPart.LeftLeg, TargetBodyPart.RightLeg,
            TargetBodyPart.LeftFoot, TargetBodyPart.RightFoot
        };

        // Allow head and torso when downed or all limbs are gone
        if (isDown || AreAllLimbsGone(target, body))
        {
            allowedParts = allowedParts.Concat(new[] {
                TargetBodyPart.Head, TargetBodyPart.Groin, TargetBodyPart.Chest
            }).ToArray();
        }

        if (!allowedParts.Contains(targetPart))
        {
            _popupSystem.PopupClient(Loc.GetString("tide-rip-invalid-part"), user, user);
            return;
        }

        // Handle torso targeting
        var actualTargetPart = targetPart;
        if (targetPart == TargetBodyPart.Groin || targetPart == TargetBodyPart.Chest)
        {
            var groinPart = _body.GetBodyChildrenOfType(target, BodyPartType.Groin).FirstOrDefault();
            var chestPart = _body.GetBodyChildrenOfType(target, BodyPartType.Chest).FirstOrDefault();

            // Try groin first, then chest
            if (groinPart != default)
            {
                actualTargetPart = TargetBodyPart.Groin;
            }
            else if (chestPart != default)
            {
                actualTargetPart = TargetBodyPart.Chest;
            }
            else
            {
                // Both gone
                var curTime = _timing.CurTime;
                var tideComp = Comp<TideStyleComponent>(user);
                if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
                {
                    _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/pierce.ogg"), target);
                    tideComp.LastSoundTime = curTime;
                }
                return;
            }
        }

        var bodyPartType = GetBodyPartFromTarget(actualTargetPart);
        var symmetry = GetSymmetryFromTarget(actualTargetPart);

        var part = _body.GetBodyChildrenOfType(target, bodyPartType, symmetry: symmetry).FirstOrDefault();
        if (part == default || !TryComp<WoundableComponent>(part.Id, out var woundable))
        {
            var curTime = _timing.CurTime;
            var tideComp = Comp<TideStyleComponent>(user);
            if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/pierce.ogg"), target);
                tideComp.LastSoundTime = curTime;
            }
            return;
        }

        if (!_body.TryGetParentBodyPart(part.Id, out var parentUid, out _) || parentUid == null)
        {
            var curTime = _timing.CurTime;
            var tideComp = Comp<TideStyleComponent>(user);

            if (curTime >= tideComp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
            {
                _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Organs/OrganDestroyed4.ogg"), target);
                tideComp.LastSoundTime = curTime;
            }

            _body.GibBody(target, gibOrgans: true);
            _popupSystem.PopupClient(Loc.GetString("tide-rip-success"), user, user);
            return;
        }


        if (TryComp<WoundableComponent>(parentUid.Value, out var parentWoundable))
        {
            if (_wounds.TryInduceWound(parentUid.Value, "Slash", 5f, out var bleedWound))
            {
                if (TryComp<BleedInflicterComponent>(bleedWound.Value.Owner, out var bleedComp))
                {
                    bleedComp.IsBleeding = true;
                    bleedComp.BleedingAmountRaw = 8f;
                    bleedComp.ScalingLimit += 8;
                    Dirty(bleedWound.Value.Owner, bleedComp);
                }
            }
        }

        _wounds.AmputateWoundable(parentUid.Value, part.Id, woundable);
        _hands.TryPickupAnyHand(user, part.Id);

        var curTime2 = _timing.CurTime;
        var tideComp2 = Comp<TideStyleComponent>(user);
        if (curTime2 >= tideComp2.LastSoundTime + TimeSpan.FromSeconds(0.1f))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/_Shitmed/ReBELL/Organs/OrganDestroyed4.ogg"), target);
            tideComp2.LastSoundTime = curTime2;
        }

        _popupSystem.PopupClient(Loc.GetString("tide-rip-success"), user, user);
    }

    private bool AreAllLimbsGone(EntityUid target, BodyComponent body)
    {
        var limbTypes = new[] { BodyPartType.Arm, BodyPartType.Leg, BodyPartType.Hand, BodyPartType.Foot, BodyPartType.Head };

        foreach (var limbType in limbTypes)
        {
            var parts = _body.GetBodyChildrenOfType(target, limbType);
            if (parts.Any())
                return false;
        }

        return true;
    }

    private void OnTideStyleShove(Entity<TideStyleComponent> ent, ref DisarmedEvent args)
    {
        if (args.Handled)
            return;

        if (!TryComp<MartialArtsKnowledgeComponent>(ent, out var knowledge) ||
            knowledge.MartialArtsForm != MartialArtsForms.TideStyle)
            return;

        var target = args.Target;
        var curTime = _timing.CurTime;

        if (curTime > ent.Comp.LastHitTime + TimeSpan.FromSeconds(ent.Comp.HitDecayTime))
        {
            ent.Comp.ConsecutiveHits = 1;
        }
        else
        {
            ent.Comp.ConsecutiveHits++;
        }

        ent.Comp.LastHitTime = curTime;
        Dirty(ent);

        var tripleStaminaDamage = args.StaminaDamage * 3f;
        _stamina.TakeStaminaDamage(target, tripleStaminaDamage, source: ent, applyResistances: true);

        var mapPos = _transform.GetMapCoordinates(ent).Position;
        var hitPos = _transform.GetMapCoordinates(target).Position;
        var dir = hitPos - mapPos;

        if (dir.Length() > 0)
            dir *= ent.Comp.ShoveDistance / dir.Length();

        _grabThrowing.Throw(target, ent, dir, 3f);

        if (curTime >= ent.Comp.LastSoundTime + TimeSpan.FromSeconds(0.1f))
        {
            _audio.PlayPvs(new SoundPathSpecifier("/Audio/Weapons/genhit1.ogg"), target);
            ent.Comp.LastSoundTime = curTime;
        }

        args.Handled = true;
    }

    private BodyPartType GetBodyPartFromTarget(TargetBodyPart target)
    {
        return target switch
        {
            TargetBodyPart.Head => BodyPartType.Head,
            TargetBodyPart.Chest => BodyPartType.Chest,
            TargetBodyPart.Groin => BodyPartType.Groin,
            TargetBodyPart.LeftArm or TargetBodyPart.RightArm => BodyPartType.Arm,
            TargetBodyPart.LeftHand or TargetBodyPart.RightHand => BodyPartType.Hand,
            TargetBodyPart.LeftLeg or TargetBodyPart.RightLeg => BodyPartType.Leg,
            TargetBodyPart.LeftFoot or TargetBodyPart.RightFoot => BodyPartType.Foot,
            _ => BodyPartType.Other
        };
    }

    private BodyPartSymmetry GetSymmetryFromTarget(TargetBodyPart target)
    {
        return target switch
        {
            TargetBodyPart.LeftArm or TargetBodyPart.LeftHand or
            TargetBodyPart.LeftLeg or TargetBodyPart.LeftFoot => BodyPartSymmetry.Left,
            TargetBodyPart.RightArm or TargetBodyPart.RightHand or
            TargetBodyPart.RightLeg or TargetBodyPart.RightFoot => BodyPartSymmetry.Right,
            _ => BodyPartSymmetry.None
        };
    }
}
