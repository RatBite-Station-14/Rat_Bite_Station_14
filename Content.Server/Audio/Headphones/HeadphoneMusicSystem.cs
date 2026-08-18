// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Audio.Headphones;
using Content.Shared.Audio.Jukebox;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Content.Shared.Item.ItemToggle;
using Content.Shared.Item.ItemToggle.Components;
using Content.Shared.Popups;
using Content.Shared.UserInterface;
using Content.Shared.Verbs;
using Robust.Server.GameObjects;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Server.Audio.Headphones;

public sealed class HeadphoneMusicSystem : EntitySystem
{
    [Dependency] private readonly IPrototypeManager _prototype = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly ItemToggleSystem _toggle = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly UserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HeadphoneMusicComponent, GetVerbsEvent<AlternativeVerb>>(OnGetVerbs);
        SubscribeLocalEvent<HeadphoneMusicComponent, ItemToggleActivateAttemptEvent>(OnActivateAttempt);
        SubscribeLocalEvent<HeadphoneMusicComponent, ItemToggledEvent>(OnToggled);
        SubscribeLocalEvent<HeadphoneMusicComponent, GotEquippedEvent>(OnGotEquipped);
        SubscribeLocalEvent<HeadphoneMusicComponent, GotUnequippedEvent>(OnGotUnequipped);
        SubscribeLocalEvent<HeadphoneMusicComponent, HeadphoneMusicSelectedMessage>(OnSongSelected);
        SubscribeLocalEvent<HeadphoneMusicComponent, HeadphoneMusicSetTimeMessage>(OnSetTime);
        SubscribeLocalEvent<HeadphoneMusicComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnGetVerbs(Entity<HeadphoneMusicComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        var user = args.User;
        args.Verbs.Add(new AlternativeVerb
        {
            Text = Loc.GetString("headphone-music-verb-show"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/dot.svg.192dpi.png")),
            Act = () => _ui.TryOpenUi(ent.Owner, HeadphoneMusicUiKey.Key, user),
        });
    }

    private void OnActivateAttempt(Entity<HeadphoneMusicComponent> ent, ref ItemToggleActivateAttemptEvent args)
    {
        if (ent.Comp.SelectedSongId != null &&
            _prototype.HasIndex<JukeboxPrototype>(ent.Comp.SelectedSongId))
        {
            return;
        }

        args.Cancelled = true;
        args.Popup = Loc.GetString("headphone-music-no-song-selected");
    }

    private void OnToggled(Entity<HeadphoneMusicComponent> ent, ref ItemToggledEvent args)
    {
        if (args.Activated)
        {
            StartPlayback(ent, args.User);
            return;
        }

        StopPlayback(ent);
    }

    private void OnGotEquipped(Entity<HeadphoneMusicComponent> ent, ref GotEquippedEvent args)
    {
        DeactivateIfRangeChanged(ent);
    }

    private void OnGotUnequipped(Entity<HeadphoneMusicComponent> ent, ref GotUnequippedEvent args)
    {
        DeactivateIfRangeChanged(ent);
    }

    private void OnSongSelected(Entity<HeadphoneMusicComponent> ent, ref HeadphoneMusicSelectedMessage args)
    {
        ent.Comp.SelectedSongId = args.SongId;
        Dirty(ent);

        if (TryComp<ItemToggleComponent>(ent, out var toggle) && toggle.Activated)
            StartPlayback(ent, args.Actor);
    }

    private void OnSetTime(Entity<HeadphoneMusicComponent> ent, ref HeadphoneMusicSetTimeMessage args)
    {
        if (ent.Comp.AudioStream == null)
            return;

        _audio.SetPlaybackPosition(ent.Comp.AudioStream, args.SongTime);
    }

    private void OnShutdown(Entity<HeadphoneMusicComponent> ent, ref ComponentShutdown args)
    {
        StopPlayback(ent);
    }

    private void StartPlayback(Entity<HeadphoneMusicComponent> ent, EntityUid? user)
    {
        StopPlayback(ent);

        if (ent.Comp.SelectedSongId == null ||
            !_prototype.TryIndex(ent.Comp.SelectedSongId, out var song))
        {
            if (user != null)
                _popup.PopupEntity(Loc.GetString("headphone-music-no-song-selected"), ent.Owner, user.Value);

            return;
        }

        var maxDistance = GetMaxDistance(ent);
        ent.Comp.AudioStream = _audio.PlayPvs(song.Path, ent.Owner, GetAudioParams(maxDistance))?.Entity;
        ent.Comp.ActiveMaxDistance = maxDistance;
        Dirty(ent);
    }

    private void StopPlayback(Entity<HeadphoneMusicComponent> ent)
    {
        ent.Comp.AudioStream = _audio.Stop(ent.Comp.AudioStream);
        ent.Comp.ActiveMaxDistance = null;
        Dirty(ent);
    }

    private void DeactivateIfRangeChanged(Entity<HeadphoneMusicComponent> ent)
    {
        if (ent.Comp.ActiveMaxDistance == null ||
            !TryComp<ItemToggleComponent>(ent, out var toggle) ||
            !toggle.Activated)
        {
            return;
        }

        if (ent.Comp.ActiveMaxDistance.Value.Equals(GetMaxDistance(ent)))
            return;

        _toggle.TryDeactivate(ent.Owner, predicted: false);
    }

    private AudioParams GetAudioParams(float maxDistance)
    {
        return AudioParams.Default
            .WithMaxDistance(maxDistance)
            .WithVolume(-6f);
    }

    private float GetMaxDistance(Entity<HeadphoneMusicComponent> ent)
    {
        if (!_inventory.TryGetContainingSlot(ent.Owner, out var slot))
            return ent.Comp.PublicMaxDistance;

        if ((slot.SlotFlags & (SlotFlags.HEAD | SlotFlags.EARS)) == 0)
            return ent.Comp.PublicMaxDistance;

        return ent.Comp.PrivateMaxDistance;
    }
}
