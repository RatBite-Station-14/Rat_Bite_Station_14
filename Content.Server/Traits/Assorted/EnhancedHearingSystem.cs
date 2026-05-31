// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Chat.Systems;
using Content.Shared.Examine;
using Robust.Server.Player;

namespace Content.Server.Traits.Assorted;

public sealed class EnhancedHearingSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly IPlayerManager _players = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<ExpandICChatRecipientsEvent>(OnExpandRecipients);
    }

    private void OnExpandRecipients(ExpandICChatRecipientsEvent args)
    {
        var enhancedRange = args.VoiceRange switch
        {
            ChatSystem.VoiceRange => 14f,
            ChatSystem.WhisperMuffledRange => 8f,
            _ => args.VoiceRange,
        };

        if (enhancedRange <= args.VoiceRange)
            return;

        var sourceXform = Transform(args.Source);

        foreach (var session in _players.Sessions)
        {
            if (args.Recipients.ContainsKey(session) ||
                session.AttachedEntity is not { Valid: true } listener ||
                !HasComp<EnhancedHearingComponent>(listener))
                continue;

            var listenerXform = Transform(listener);
            if (listenerXform.MapID != sourceXform.MapID ||
                !sourceXform.Coordinates.TryDistance(EntityManager, listenerXform.Coordinates, out var distance) ||
                distance > enhancedRange)
                continue;

            var inLos = _examine.InRangeUnOccluded(args.Source, listener, enhancedRange);
            args.Recipients.Add(session, new ChatSystem.ICChatRecipientData(distance, false, InLOS: inLos));
        }
    }
}
