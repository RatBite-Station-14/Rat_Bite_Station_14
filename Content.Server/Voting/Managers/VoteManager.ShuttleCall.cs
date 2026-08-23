// Ratbite file

using Content.Server.RoundEnd;
using Content.Shared._BRatbite.CCVar;
using Content.Shared.CCVar;
using Content.Shared.Voting;
using Robust.Shared.Player;

namespace Content.Server.Voting.Managers;

public sealed partial class VoteManager
{

    private void CreateShuttleCallVote(ICommonSession? initiator)
    {
        var alone = _playerManager.PlayerCount == 1 && initiator != null;
        var requiredRatio = _cfg.GetCVar(RatbiteCVars.VoteShuttleRequiredRatio);
        var options = new VoteOptions
        {
            Title = Loc.GetString("ui-vote-shuttle-call-title", [("requiredRatio", requiredRatio.ToString("0%"))]),
            Options =
            {
                (Loc.GetString("ui-vote-restart-yes"), "yes"),
                (Loc.GetString("ui-vote-restart-no"), "no"),
                (Loc.GetString("ui-vote-restart-abstain"), "abstain")
            },
            Duration = alone
            ? TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerAlone))
            : TimeSpan.FromSeconds(_cfg.GetCVar(CCVars.VoteTimerRestart)),
            InitiatorTimeout = TimeSpan.FromMinutes(5),
            AllowMultiple = false,
        };
        if (alone)
            options.InitiatorTimeout = TimeSpan.FromSeconds(10);
        TimeoutStandardVote(StandardVoteType.ShuttleCall, TimeSpan.FromMinutes(_cfg.GetCVar(RatbiteCVars.VoteShuttleCooldownMinutes)));
        WirePresetVoteInitiator(options, initiator);
        var vote = CreateVote(options);
        vote.OnFinished += (_, _) =>
        {
            var voteYes = vote.VotesPerOption["yes"];
            var voteNo = vote.VotesPerOption["no"];
            var total = voteYes + voteNo;
            var roundEndSystem = _entityManager.System<RoundEndSystem>();

            if (total > 0 && voteYes / (float) total >= requiredRatio && !roundEndSystem.IsRoundEndRequested())
            {
                _entityManager.System<RoundEndSystem>().RequestRoundEnd(text: "vote-shuttle-call-announcement-desc");
            }
        };
        if (initiator != null)
        {
            // Cast yes vote if created the vote yourself.
            vote.CastVote(initiator, 0);
        }

        foreach (var player in _playerManager.Sessions)
        {
            if (player != initiator)
            {
                // Everybody else defaults to an abstain vote to say they don't mind.
                vote.CastVote(player, 2);
            }
        }

    }
}
