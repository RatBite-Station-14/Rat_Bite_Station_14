// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Chat;
using Content.Shared.IdentityManagement;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server._BRatbite.Chat;

public sealed class IdentityChatSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public void TrySendInGameICMessageAsIdentity(
        EntityUid source,
        string message,
        InGameICChatType desiredType,
        bool hideChat,
        bool hideLog = false,
        IConsoleShell? shell = null,
        ICommonSession? player = null,
        bool checkRadioPrefix = true,
        bool ignoreActionBlocker = false,
        Color? colorOverride = null)
    {
        var identity = Identity.Entity(source, EntityManager);
        var nameOverride = Name(identity);

        _chat.TrySendInGameICMessage(
            source,
            message,
            desiredType,
            hideChat,
            hideLog,
            shell,
            player,
            nameOverride,
            checkRadioPrefix,
            ignoreActionBlocker,
            colorOverride);
    }
}
