// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._BRatbite.Nutrition;
using Content.Shared._BRatbite.Speech;
using Content.Shared.StatusEffectNew;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// This handles removing accents when using the accentless trait.
/// </summary>
public sealed class AccentlessSystem : EntitySystem
{
    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<AccentlessComponent, OnBeforeAccentEvent>(OnBeforeAccent);
        SubscribeLocalEvent<AccentlessComponent, StatusEffectRelayedEvent<OnBeforeAccentEvent>>(OnBeforeAccentRelayed);
        SubscribeLocalEvent<AccentlessComponent, EffectDescriptionEvent>(OnGetDescription);
    }

    private void OnBeforeAccent(Entity<AccentlessComponent> ent, ref OnBeforeAccentEvent args)
    {
        args.Cancelled = true;
    }

    private void OnBeforeAccentRelayed(Entity<AccentlessComponent> ent, ref StatusEffectRelayedEvent<OnBeforeAccentEvent> args)
    {
        args.Args = args.Args with { Cancelled = true };
    }

    private void OnGetDescription(Entity<AccentlessComponent> ent, ref EffectDescriptionEvent args)
    {
        args.Message.AddMarkupOrThrow(Loc.GetString("guidebook-description-accentless"));
        args.Message.PushNewline();
    }
}
