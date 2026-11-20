using System.Linq;
using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Server.GameTicking.Rules;
using Content.Server.Mind;
using Content.Server.Roles.Jobs;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mind;
using Content.Shared.Roles;
using Robust.Server.Player;
using Robust.Shared.Enums;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.GameTicking.Rules;

public sealed class SusRatThreatSystem : GameRuleSystem<SusRatThreatComponent>
{
    private sealed record MidroundOption(
        string Rule,
        int Max,
        string[] CountedRules,
        int? MinimumPopulation = null,
        int? MaximumPopulation = null,
        bool ChaplainNeeded = false,
        bool PreserveTeamSize = false);

    [Dependency] private readonly AntagSelectionSystem _antag = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly JobSystem _jobs = default!;
    [Dependency] private readonly MindSystem _mind = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    private static readonly MidroundOption[] MidroundOptions =
    [
        new("TraitorMidround", 4, ["SusRatTraitor", "Traitor", "TraitorMidround"]),
        new("ChangelingMidround", 3, ["SusRatChangeling", "Changeling", "ChangelingMidround"]),
        // new("Shadowling", 2, ["Shadowling"]), -- Experienced players mentioned that Shadowlings are too overtuned.
        new("ParadoxCloneSpawn", 3, ["ParadoxCloneSpawn"]),
        new("Devil", 1, ["Devil"]),
        new("DarkPriestMidround", 1, ["DarkPriestMidround"]),
        new("NinjaSpawn", 1, ["NinjaSpawn"], MaximumPopulation: 40),
        new("LoneOpsSpawn", 2, ["LoneOpsSpawn"], MinimumPopulation: 40),
        new("HereticMidround", 2, ["SusRatHeretic", "Heretic", "HereticMidround"], MinimumPopulation: 40, ChaplainNeeded: true),
        new("Xenoborgs", 1, ["Xenoborgs"], MinimumPopulation: 40, PreserveTeamSize: true),
        new("LoneAbductorSpawn", 1, ["LoneAbductorSpawn"], MinimumPopulation: 25),
        new("DuoAbductorSpawn", 1, ["DuoAbductorSpawn"], MinimumPopulation: 30, PreserveTeamSize: true),
        new("BingleSpawn", 1, ["BingleSpawn"], MinimumPopulation: 45, PreserveTeamSize: true),
        new("SlasherSpawn", 2, ["SlasherSpawn"], MinimumPopulation: 45),
        new("VoxRaidersMidround", 1, ["VoxRaidersMidround"], MinimumPopulation: 30, PreserveTeamSize: true),
        new("RevolutionaryMidround", 1, ["RevolutionaryMidround"], MinimumPopulation: 60, PreserveTeamSize: true),
    ];

    private static readonly string[] RoundEnders =
    [
        "Nukeops",
        "Honkops",
        "ZombieOutbreak",
        "DragonSpawn",
        "ColossusSpawn",
        "CosmicCult",
        "WraithMidround",
        "BlobSpawn",
        "XenomorphsInfestation",
    ];

    private static readonly ProtoId<JobPrototype> ChaplainJob = "Chaplain";

    protected override void Started(EntityUid uid, SusRatThreatComponent component, GameRuleComponent gameRule,
        GameRuleStartedEvent args)
    {
        base.Started(uid, component, gameRule, args);

        component.RoundEnderRolled = false;
        component.MidroundTime = _timing.CurTime + Vary(component.MidroundDelay, component.MidroundVariance);
        component.RoundEnderTime = _timing.CurTime + Vary(component.RoundEnderDelay, component.RoundEnderVariance);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = QueryActiveRules();
        while (query.MoveNext(out _, out var component, out _))
        {
            if (_timing.CurTime >= component.MidroundTime)
            {
                RollMidrounds();
                component.MidroundTime = _timing.CurTime + Vary(component.MidroundDelay, component.MidroundVariance);
            }

            if (!component.RoundEnderRolled && _timing.CurTime >= component.RoundEnderTime)
            {
                component.RoundEnderRolled = true;
                RollRoundEnders();
            }
        }
    }

    private TimeSpan Vary(TimeSpan delay, float variance)
    {
        return delay * _random.NextFloat(1f - variance, 1f + variance);
    }

    private void RollMidrounds()
    {
        var population = _antag.GetTotalPlayerCount(_players.Sessions);
        var chaplainPresent = HasAliveJob(ChaplainJob);
        var eligible = MidroundOptions
            .Where(option => (option.MinimumPopulation == null || population >= option.MinimumPopulation)
                && (option.MaximumPopulation == null || population <= option.MaximumPopulation)
                && (!option.ChaplainNeeded || chaplainPresent))
            .ToList();

        _random.Shuffle(eligible);
        var typeCount = Math.Min(_random.Next(1, 4), eligible.Count);
        var globalMax = Math.Max(0, (int) MathF.Round(population * 0.8f / 10f, MidpointRounding.AwayFromZero));
        var remainingGlobal = Math.Max(0, globalMax - CountAliveAntags());

        foreach (var option in eligible.Take(typeCount))
        {
            if (remainingGlobal <= 0)
                break;

            var remainingForType = Math.Max(0, option.Max - CountExisting(option));
            var rollMax = Math.Min(remainingForType, remainingGlobal);
            var count = _random.Next(rollMax + 1);
            if (count == 0)
                continue;

            if (option.PreserveTeamSize)
            {
                StartRule(option.Rule);
                remainingGlobal--;
                continue;
            }

            StartRule(option.Rule, count);
            remainingGlobal -= count;
        }
    }

    private void RollRoundEnders()
    {
        var primary = _random.Pick(RoundEnders);
        StartRoundEnder(primary);

        if (!_random.Prob(0.1f))
            return;

        var additional = RoundEnders.Where(rule => rule != primary).ToList();
        StartRoundEnder(_random.Pick(additional));
    }

    private void StartRoundEnder(string rule)
    {
        StartRule(rule);
        if (rule == "DragonSpawn" && _random.Prob(0.2f))
            StartRule(rule);
    }

    private void StartRule(string rule, int? targetCount = null)
    {
        var ruleEntity = GameTicker.AddGameRule(rule);
        if (targetCount != null && TryComp<AntagSelectionComponent>(ruleEntity, out var selection))
        {
            for (var i = 0; i < selection.Definitions.Count; i++)
            {
                var definition = selection.Definitions[i];
                definition.Min = targetCount.Value;
                definition.Max = targetCount.Value;
                selection.Definitions[i] = definition;
            }
        }

        GameTicker.StartGameRule(ruleEntity);
    }

    private int CountExisting(MidroundOption option)
    {
        var count = 0;
        var query = EntityQueryEnumerator<MetaDataComponent>();
        while (query.MoveNext(out var uid, out var metadata))
        {
            if (metadata.EntityPrototype?.ID is not { } prototype || !option.CountedRules.Contains(prototype))
                continue;

            if (TryComp<AntagSelectionComponent>(uid, out var selection))
                count += _antag.GetAliveAntagCount((uid, selection));
            else if (HasComp<ActiveGameRuleComponent>(uid) || HasComp<EndedGameRuleComponent>(uid))
                count++;
        }

        return count;
    }

    private int CountAliveAntags()
    {
        var antags = new HashSet<EntityUid>();
        var query = EntityQueryEnumerator<AntagSelectionComponent>();
        while (query.MoveNext(out var uid, out var selection))
        {
            foreach (var antag in _antag.GetAliveAntags((uid, selection)))
                antags.Add(antag);
        }

        return antags.Count;
    }

    private bool HasAliveJob(ProtoId<JobPrototype> job)
    {
        return AliveConnectedJobs().Any(entry => entry.Job.ID == job);
    }

    private List<(EntityUid Mind, JobPrototype Job)> AliveConnectedJobs()
    {
        var jobs = new List<(EntityUid, JobPrototype)>();
        foreach (var session in _players.Sessions)
        {
            if (session.Status is SessionStatus.Disconnected or SessionStatus.Zombie
                || !_mind.TryGetMind(session, out var mindId, out var mind)
                || _mind.IsCharacterDeadIc(mind)
                || !_jobs.MindTryGetJob(mindId, out var job))
            {
                continue;
            }

            jobs.Add((mindId, job));
        }

        return jobs;
    }
}
