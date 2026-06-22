// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aviu00 <93730715+Aviu00@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Misandry <mary@thughunt.ing>
// SPDX-FileCopyrightText: 2025 Spatison <137375981+Spatison@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 gus <august.eymann@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.Changeling.Components;
using Content.Goobstation.Shared.Overlays;
using Content.Shared.Body.Systems;
using Content.Shared.Eye.Blinding.Components;
using Content.Shared.CCVar;
using Content.Shared.Chemistry.Hypospray.Events;
using Content.Shared.Climbing.Components;
using Content.Shared.Climbing.Events;
using Content.Shared.Damage;
using Content.Shared.IdentityManagement;
using Content.Shared.Medical;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Goobstation.Shared.Changeling.Systems;

public abstract class SharedChangelingSystem : EntitySystem
{
    [Dependency] protected readonly SharedBodySystem Body = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ChangelingIdentityComponent, SwitchableOverlayToggledEvent>(OnVisionToggle);
        SubscribeLocalEvent<ChangelingIdentityComponent, SelfBeforeGunShotEvent>(BeforeGunShotEvent);
    }

    private void OnVisionToggle(Entity<ChangelingIdentityComponent> ent, ref SwitchableOverlayToggledEvent args)
    {
        if (args.User != ent.Owner)
            return;

        if (TryComp(ent, out EyeProtectionComponent? eyeProtection))
            eyeProtection.ProtectionTime = args.Activated ? TimeSpan.Zero : TimeSpan.FromSeconds(10);

        UpdateFlashImmunity(ent, !args.Activated);
    }

    protected virtual void UpdateFlashImmunity(EntityUid uid, bool active) { }

    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly INetManager _net = default!;

    // If you add more clumsy interactions add them in this section!
    #region Clumsy interaction events
    private void BeforeGunShotEvent(Entity<ChangelingIdentityComponent> ent, ref SelfBeforeGunShotEvent args)
    {
        // Clumsy people sometimes can't shoot :(

        // checks if ClumsyGuns is false, if so, skips.
        if (!ent.Comp.ClumsyGuns)
            return;

        if (args.Gun.Comp.ClumsyProof)
            return;

        // TODO: Replace with RandomPredicted once the engine PR is merged
        var seed = SharedRandomExtensions.HashCodeCombine(new() { (int)_timing.CurTick.Value, GetNetEntity(args.Gun).Id });
        var rand = new System.Random(seed);
        if (!rand.Prob(ent.Comp.ClumsyDefaultCheck))
            return;

        if (ent.Comp.GunShootFailDamage != null)
            _damageable.TryChangeDamage(ent, ent.Comp.GunShootFailDamage, origin: ent);

        _stun.TryParalyze(ent, ent.Comp.GunShootFailStunTime, true);

        // Apply salt to the wound ("Honk!") (No idea what this comment means)
        _audio.PlayPvs(ent.Comp.GunShootFailSound, ent);
        _audio.PlayPvs(ent.Comp.ClumsySound, ent);

        _popup.PopupEntity(Loc.GetString(ent.Comp.GunFailedMessage), ent, ent);
        args.Cancel();
    }
    #endregion
}
