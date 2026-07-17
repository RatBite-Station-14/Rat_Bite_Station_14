// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 Timfa <timfalken@hotmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

// SPDX-FileCopyrightText: 2025 Baptr0b0t <152836416+Baptr0b0t@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 GoobBot <uristmchands@proton.me>
// SPDX-FileCopyrightText: 2025 Ted Lukin <66275205+pheenty@users.noreply.github.com>

// SPDX-FileCopyrightText: 2025 Tim <timfalken@hotmail.com>
// SPDX-FileCopyrightText: 2025 pheenty <fedorlukin2006@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Server.Medical.CrewMonitoring;
using Content.Server.Pinpointer;
using Content.Shared._BRatbite.TrackingHud;
using Content.Shared.Chat;
using Content.Shared.Mobs;
using Robust.Server.GameObjects;
using Robust.Shared.Utility;

namespace Content.Goobstation.Server.RelayedDeathrattle;

public sealed class RelayedDeathrattleSystem : EntitySystem
{
    [Dependency] private readonly NavMapSystem _navMap = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly SharedTrackingTargetSystem _trackingTargetSystem = default!;
    [Dependency] private readonly TransformSystem _transform = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<RelayedDeathrattleComponent, MobStateChangedEvent>(OnMobStateChanged);
    }

    private void OnMobStateChanged(EntityUid uid, RelayedDeathrattleComponent comp, MobStateChangedEvent args)
    {
        if (comp.Target == null)
            return;


        bool dead;
        var posText = FormattedMessage.RemoveMarkupOrThrow(_navMap.GetNearestBeaconString(uid));
        if (args is { NewMobState: MobState.Critical, OldMobState: MobState.Alive })
            dead = false;
        else if (args.NewMobState == MobState.Dead)
            dead = true;
        else
            return;
        // Ratbite start
        var coordinates = _transform.GetMapCoordinates(Transform(uid));
        _trackingTargetSystem.AddTargetToAllEntities(new TrackingTarget
        {
            TargetLocation = coordinates.Position,
            MapId = coordinates.MapId,
            Sprite = new SpriteSpecifier.Rsi(new("/Textures/Effects/crayondecals.rsi"), "exclamationmark")
        }, deleteAfter: TimeSpan.FromSeconds(30));
        // Ratbite end
        _chat.TrySendInGameICMessage(comp.Target.Value, Loc.GetString(dead ? comp.DeathMessage : comp.CritMessage, ("user", uid), ("position", posText)), InGameICChatType.Speak, hideChat: false);
    }
}
