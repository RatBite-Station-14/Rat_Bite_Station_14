using Content.Goobstation.Common.Speech;
using Content.Shared._BRatbite.Atmos;
using Content.Shared._BRatbite.DoAfter;
using Content.Shared._BRatbite.Nutrition;
using Content.Shared._BRatbite.ServerCurrency;
using Content.Shared._BRatbite.Speech;
using Content.Shared._EinsteinEngines.Language.Events;
using Content.Shared._Shitmed.DoAfter;
using Content.Shared.Chat;
using Content.Shared.Cuffs.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory.Events;
using Content.Shared.Overlays;
using Content.Shared.Slippery;
using Content.Shared.Speech;
using Content.Shared.Standing;
using Content.Shared.StatusEffectNew.Components;
using Content.Shared.StepTrigger.Systems;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared.StatusEffectNew;

public sealed partial class StatusEffectsSystem
{
    private void SubscribeRatbiteRelays()
    {
        SubscribeLocalEvent<StatusEffectContainerComponent, GunRefreshModifiersEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, GetMeleeDamageEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, GetDoAfterDelayMultiplierEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, TransformSpeakerFontEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, DamageModifyEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, HungerMultiplierEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, FellDownThrowAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, SlipAttemptEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, GetSlowedOverSlipperyModifierEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, SpeakAttemptEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, GetEmoteSoundsEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, ServerCurrencyMultiplierEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, GetDoAfterTargetMultiplierEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, RefreshEquipmentHudEvent<ShowJobIconsComponent>>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, OnBeforeAccentEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, StepTriggeredOffEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, StepTriggerAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, CuffAttemptEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, DetermineEntityLanguagesEvent>(RefRelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, AttackedEvent>(RelayStatusEffectEvent);
        SubscribeLocalEvent<StatusEffectContainerComponent, ExtinguishAttemptEvent>(RefRelayStatusEffectEvent);
    }
}
