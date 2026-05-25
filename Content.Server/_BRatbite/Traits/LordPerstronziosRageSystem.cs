// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Body.Systems;
using Content.Server.Chat.Systems;
using Content.Shared._BRatbite.Traits;
using Content.Shared.Actions;
using Content.Shared.Chat;
using Content.Shared.Mind.Components;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Traits;

public sealed class LordPerstronziosRageSystem : EntitySystem
{
    private static readonly TimeSpan MaldGibDelay = TimeSpan.FromSeconds(2.5);

    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly BodySystem _body = default!;
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LordPerstronziosRageComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<LordPerstronziosRageComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<LordPerstronziosRageComponent, PlayerAttachedEvent>(OnPlayerAttached);
        SubscribeLocalEvent<LordPerstronziosRageComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<LordPerstronziosRageComponent, MaldActionEvent>(OnMald);
    }

    private void OnStartup(Entity<LordPerstronziosRageComponent> ent, ref ComponentStartup args)
    {
        EnsureRage(ent);
    }

    private void OnShutdown(Entity<LordPerstronziosRageComponent> ent, ref ComponentShutdown args)
    {
        _actions.RemoveAction(ent.Owner, ent.Comp.ActionEntity);
        ent.Comp.ActionEntity = null;
    }

    private void OnPlayerAttached(Entity<LordPerstronziosRageComponent> ent, ref PlayerAttachedEvent args)
    {
        EnsureRage(ent);
    }

    private void OnMindAdded(Entity<LordPerstronziosRageComponent> ent, ref MindAddedMessage args)
    {
        EnsureRage(ent);
    }

    private void EnsureRage(Entity<LordPerstronziosRageComponent> ent)
    {
        _actions.AddAction(ent.Owner, ref ent.Comp.ActionEntity, ent.Comp.Action);
    }

    private void OnMald(Entity<LordPerstronziosRageComponent> ent, ref MaldActionEvent args)
    {
        if (args.Handled)
            return;

        _chat.TrySendInGameICMessage(
            ent.Owner,
            "I'm not malding, I can't mald! I'm still alive!",
            InGameICChatType.Speak,
            hideChat: false,
            checkRadioPrefix: false,
            ignoreActionBlocker: true);

        _chat.TryEmoteWithChat(ent.Owner, "Fart", ignoreActionBlocker: true, forceEmote: true);

        var uid = ent.Owner;
        Timer.Spawn(MaldGibDelay, () =>
        {
            if (TerminatingOrDeleted(uid))
                return;

            _body.GibBody(uid);
        });

        args.Handled = true;
    }
}
