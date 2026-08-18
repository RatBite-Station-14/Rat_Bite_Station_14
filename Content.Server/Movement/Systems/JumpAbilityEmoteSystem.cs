// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Server.Movement.Systems;

public sealed class JumpAbilityEmoteSystem : EntitySystem
{
    [Dependency] private readonly ChatSystem _chat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<JumpAbilityComponent, JumpAbilityPerformedEvent>(OnJumpAbilityPerformed);
    }

    private void OnJumpAbilityPerformed(Entity<JumpAbilityComponent> ent, ref JumpAbilityPerformedEvent args)
    {
        _chat.TryEmoteWithChat(
            ent.Owner,
            "Flip",
            ignoreActionBlocker: true,
            forceEmote: true,
            voluntary: false);
    }
}
