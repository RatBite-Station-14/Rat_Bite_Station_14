// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Shitmed.Targeting;
using Robust.Shared.GameStates;
using Robust.Shared.Serialization;

namespace Content.Goobstation.Shared.MartialArts.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class TideStyleComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntityUid> TideStyleAbilityEntities = new();

    [DataField, AutoNetworkedField]
    public TideStyleAbility? SelectedAbility;

    [DataField, AutoNetworkedField]
    public TargetBodyPart? SelectedBodyPart;

    [DataField, AutoNetworkedField]
    public bool PendingBodyPartSelection;

    [DataField, AutoNetworkedField]
    public bool AbilityReady;

    [DataField, AutoNetworkedField]
    public ushort? DoAfterId;

    [DataField, AutoNetworkedField]
    public TimeSpan? DoAfterCompletionTime;

    [DataField, AutoNetworkedField]
    public bool SecondDoAfter;

    // Hit stack
    [DataField, AutoNetworkedField]
    public int ConsecutiveHits = 0;

    [DataField, AutoNetworkedField]
    public TimeSpan LastHitTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan LastPushTime = TimeSpan.Zero;

    [DataField]
    public TimeSpan LastSoundTime = TimeSpan.Zero;

    // Values to tune
    [DataField]
    public float MovementSpeedModifier = 0.7f;

    [DataField]
    public float PunchDamage = 5f;

    [DataField]
    public float PushDamage = 15f;

    [DataField]
    public float PushKnockbackDistance = 4f;

    [DataField]
    public float PushStunTime = 2f;

    [DataField]
    public float PushCooldown = 7f;

    [DataField]
    public float ShoveDistance = 2f;

    [DataField]
    public float BoneBrokenChance = 0.15f;

    [DataField]
    public float RipDelayBase = 4f;

    [DataField]
    public float RipDelayVariation = 0.5f;

    [DataField]
    public float StompDelayBase = 2.5f;

    [DataField]
    public float StompDelayVariation = 0.5f;

    [DataField]
    public float AttackWindowStart = -0.5f;

    [DataField]
    public float AttackWindowEnd = 0.5f;

    [DataField]
    public float HitDelayReductionPerHit = 0.3f;

    public float MaxHitDelayReduction = 1.5f;

    [DataField]
    public float HitCooldownReductionPerHit = 0.5f;

    [DataField]
    public float MaxHitCooldownReduction = 3.0f;

    [DataField]
    public float HitDecayTime = 3.0f;
}

[Serializable, NetSerializable]
public enum TideStyleAbility : byte
{
    Rip,
    Stomp,
    Push
}
