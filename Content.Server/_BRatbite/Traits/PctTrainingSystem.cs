// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server._BRatbite.Chat;
using Content.Server.Chat.Systems;
using Content.Shared._BRatbite.Traits;
using Content.Shared._White.Grab;
using Content.Shared.Chat;

namespace Content.Server._BRatbite.Traits;

public sealed class PctTrainingSystem : EntitySystem
{
    [Dependency] private readonly IdentityChatSystem _identityChat = default!;
    [Dependency] private readonly ChatSystem _chat = default!;
    [Dependency] private readonly GrabThrownSystem _grabThrown = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PctTrainingComponent, PctTrainingFumbleEvent>(OnFumble);
        SubscribeLocalEvent<PctTrainingComponent, PctTrainingKnockoutEvent>(OnKnockout);
    }

    private void OnFumble(Entity<PctTrainingComponent> ent, ref PctTrainingFumbleEvent args)
    {
        _identityChat.TrySendInGameICMessageAsIdentity(
            ent.Owner,
            Loc.GetString(args.Speech),
            InGameICChatType.Speak,
            hideChat: false,
            checkRadioPrefix: false,
            ignoreActionBlocker: true);
    }

    private void OnKnockout(Entity<PctTrainingComponent> ent, ref PctTrainingKnockoutEvent args)
    {
        _identityChat.TrySendInGameICMessageAsIdentity(
            args.User,
            "Kya!",
            InGameICChatType.Speak,
            hideChat: false,
            checkRadioPrefix: false,
            ignoreActionBlocker: true);

        _chat.TryEmoteWithChat(
            args.User,
            "Flip",
            ignoreActionBlocker: true,
            forceEmote: true,
            voluntary: false);

        _grabThrown.Throw(
            args.Target,
            args.User,
            args.Direction * args.ThrowDistance,
            args.ThrowSpeed);
    }
}
