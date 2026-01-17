// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.AltTheManWhoSoldTheWorld;
using Content.Shared.Clothing.Components;
using Content.Shared.Clothing.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Item;
using Content.Shared.Mobs;
using Content.Shared.Smoking;
using Content.Shared.Verbs;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.GameStates;
using Robust.Shared.Network;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Goobstation.Shared.HoloCigar;

/// <summary>
/// This is the system for the IMPROVISED Holo-Cigar (most of this shitcode is from HoloCigarSystem.cs).
/// </summary>
public sealed class ImprovisedHoloCigarSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly ClothingSystem _clothing = default!;
    [Dependency] private readonly SharedItemSystem _items = default!;
    [Dependency] private readonly IGameTiming _gameTiming = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;

    private const string LitPrefix = "lit";
    private const string UnlitPrefix = "unlit";
    private const string MaskSlot = "mask";

    public override void Initialize()
    {
        SubscribeLocalEvent<ImprovisedHoloCigarComponent, GetVerbsEvent<AlternativeVerb>>(OnAddInteractVerb);
        SubscribeLocalEvent<ImprovisedHoloCigarComponent, ComponentHandleState>(OnComponentHandleState);

        SubscribeLocalEvent<AltTheManWhoSoldTheWorldComponent, MapInitEvent>(OnMapInitEvent);
        SubscribeLocalEvent<AltTheManWhoSoldTheWorldComponent, ComponentShutdown>(OnComponentShutdown);
        SubscribeLocalEvent<AltTheManWhoSoldTheWorldComponent, MobStateChangedEvent>(OnMobStateChangedEvent);
    }

    private void OnAddInteractVerb(Entity<ImprovisedHoloCigarComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract || args.Hands is null)
            return;

        AlternativeVerb verb = new()
        {
            Act = () =>
            {
                HandleToggle(ent);
                ent.Comp.Lit = !ent.Comp.Lit;
                Dirty(ent);
            },
            Message = Loc.GetString("holo-cigar-verb-desc"),
            Icon = new SpriteSpecifier.Texture(new ResPath("/Textures/Interface/VerbIcons/clock.svg.192dpi.png")),
            Text = Loc.GetString("holo-cigar-verb-text"),
        };

        args.Verbs.Add(verb);
    }

    private void OnMobStateChangedEvent(Entity<AltTheManWhoSoldTheWorldComponent> ent, ref MobStateChangedEvent args)
    {
        if (!TryComp<ImprovisedHoloCigarComponent>(ent.Comp.HoloCigarEntity, out var holoCigarComponent))
            return;

        if (args.NewMobState == MobState.Dead)
            _audio.Stop(holoCigarComponent.MusicEntity);

        if (_net.IsServer)
            _audio.PlayPvs(ent.Comp.DeathAudio, ent, AudioParams.Default.WithVolume(3f));
    }

    private void OnComponentShutdown(Entity<AltTheManWhoSoldTheWorldComponent> ent, ref ComponentShutdown args)
    {
        if (!TryComp<ImprovisedHoloCigarComponent>(ent.Comp.HoloCigarEntity, out var holoCigarComponent))
            return;

        _audio.Stop(holoCigarComponent.MusicEntity);
        ent.Comp.HoloCigarEntity = null;
    }

    private void OnMapInitEvent(Entity<AltTheManWhoSoldTheWorldComponent> ent, ref MapInitEvent args)
    {
        if (!_inventory.TryGetSlotEntity(ent, MaskSlot, out var cigarEntity) ||
            !HasComp<ImprovisedHoloCigarComponent>(cigarEntity))
            return;
        ent.Comp.HoloCigarEntity = cigarEntity;
    }

    private void HandleToggle(Entity<ImprovisedHoloCigarComponent> ent,
        AppearanceComponent? appearance = null,
        ClothingComponent? clothing = null)
    {
        if (!Resolve(ent, ref appearance, ref clothing) ||
            !_gameTiming.IsFirstTimePredicted)
            return;

        var state = ent.Comp.Lit ? SmokableState.Unlit : SmokableState.Lit;
        var prefix = ent.Comp.Lit ? UnlitPrefix : LitPrefix;

        _appearance.SetData(ent, SmokingVisuals.Smoking, state, appearance);
        _clothing.SetEquippedPrefix(ent, prefix, clothing);
        _items.SetHeldPrefix(ent, prefix);

        if (!_net.IsServer)
            return;

        if (ent.Comp.Lit == false)
        {
            var audio = _audio.PlayPvs(ent.Comp.Music, ent);

            if (audio is null)
                return;
            ent.Comp.MusicEntity = audio.Value.Entity;
            return;
        }

        _audio.Stop(ent.Comp.MusicEntity);
    }

    private void OnComponentHandleState(Entity<ImprovisedHoloCigarComponent> ent, ref ComponentHandleState args)
    {
        if (args.Current is not ImprovisedHoloCigarComponentState state)
            return;

        if (ent.Comp.Lit == state.Lit)
            return;

        ent.Comp.Lit = state.Lit;
        HandleToggle(ent);
    }
}
