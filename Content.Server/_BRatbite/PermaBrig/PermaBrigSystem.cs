using Content.Server.Administration.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Server.StationRecords.Systems;
using Content.Shared.Access;
using Content.Shared.Access.Components;
using Content.Shared.Access.Systems;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Inventory;
using Content.Shared.Mind;
using Content.Shared.Preferences;
using Content.Shared.Players;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Content.Shared.Security.Components;
using Content.Shared._BRatbite.PermaBrig;
using Content.Server.Traits;
using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Hands.EntitySystems;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
using Content.Server._BRatbite.CryoSickness;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Body.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._BRatbite.PermaBrig;

/// <summary>
/// This handles...
/// </summary>
public sealed class PermaBrigSystem : GameRuleSystem<PermaBrigComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PlayTimeTrackingSystem _playTimeTrackings = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly StationRecordsSystem _stationRecords = default!;
    [Dependency] private readonly AdminSystem _admin = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedIdCardSystem _idCard = default!;
    [Dependency] private readonly EntityManager _ent = default!;
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly TraitSystem _trait = default!;
    [Dependency] private readonly CryoSicknessSystem _cryoSicknessSystem = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainerSystem = default!;
    private readonly ProtoId<ReagentPrototype> _ketamine = "Ketamine";
    // This is the equivalent of 10 minutes of sedation
    private readonly FixedPoint2 _amountToInject = 2f;

    public HashSet<ICommonSession> PermaIndividuals = new();
    public Dictionary<ICommonSession, (TimeSpan, TimeSpan)> PermaIndividualJoinedTime = new();
    private ISawmill _sawmill = default!;

    private SoundSpecifier? _lockUpSound = new SoundPathSpecifier("/Audio/_BRatbite/PermaBrig/locked_up.ogg");

    /// <inheritdoc/>
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnPlayerSpawning);
        SubscribeLocalEvent<PlayerBeforeSpawnEvent>(OnPlayerBeforeSpawning);
        //SubscribeLocalEvent<RoundEndMessageEvent>(OnRoundEnd); Auto decreasing of

        _sawmill = Logger.GetSawmill("server_permabrig");
    }

    private void OnPlayerSpawning(RulePlayerSpawningEvent args)
    {
        var pool = args.PlayerPool;

        PermaIndividuals = new();

        if (!_ticker.IsGameRuleActive<PermaBrigComponent>())
            return;

        foreach (var session in pool)
        {
            if (_permaBrigManager.GetBrigTime(session.UserId) == 0)
                continue;
            PermaIndividuals.Add(session);
            _sawmill.Info($"Player intercepted for perma: {session}");
        }

        foreach (var player in PermaIndividuals)
        {
            pool.Remove(player);
            GameTicker.PlayerJoinGame(player);

            SpawnPrisonerPlayer(player, _permaBrigManager.GetBrigInpatient(player.UserId));

            _sawmill.Info($"Player sent to perma: {player}");
        }
    }

    private void OnPlayerBeforeSpawning(PlayerBeforeSpawnEvent ev)
    {
        if (!ev.LateJoin) //OnPlayerSpawning handles the start round spawning, before traitor picking, so this just needs to handle late joiners.
            return;


        if (!_ticker.IsGameRuleActive<PermaBrigComponent>())
            return;

        if (_permaBrigManager.GetBrigTime(ev.Player.UserId) == 0)
            return;

        PermaIndividuals.Add(ev.Player);

        SpawnPrisonerPlayer(ev.Player, _permaBrigManager.GetBrigInpatient(ev.Player.UserId));

        ev.Handled = true;

        _sawmill.Info($"Player sent to perma: {ev.Player}");
    }

    private EntityCoordinates? GetSpawnLocation(string jobId)
    {
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while (points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType == SpawnPointType.Job &&
                spawnPoint.Job == jobId)
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        if (possiblePositions.Count == 0)
            return null;

        return _random.Pick(possiblePositions);
    }

    private void SpawnPrisonerPlayer(ICommonSession player, bool inpatient)
    {
        var stations = _ticker.GetSpawnableStations();
        _random.Shuffle(stations);
        var station = EntityUid.Invalid;
        if (stations.Count != 0)
            station = stations[0];

        var character = _ticker.GetPlayerProfile(player);
        var forcedSpawnProfile = character.WithSpawnPriorityPreference(SpawnPriorityPreference.None);

        var data = player.ContentData();

        var newMind = _mind.CreateMind(data!.UserId, character.Name);
        _mind.SetUserId(newMind, data.UserId);

        var jobId = inpatient ? "SanitariumPatient" : "Prisoner";

        _playTimeTrackings.PlayerRolesChanged(player);

        EntityCoordinates? spawnLoc = null;
        EntityUid? mobMaybe = null;

        spawnLoc = GetSpawnLocation(jobId);

        if (inpatient && spawnLoc == null)
        {
            _sawmill.Warning("No spawn loc found for sanitarium patient");
            // If no sanitarium spawnpoint exists, use Prisoner spawn routing instead of station fallback.
            jobId = "Prisoner";
            spawnLoc = GetSpawnLocation(jobId);
        }

        var jobPrototype = _prototypeManager.Index<JobPrototype>(jobId);

        if (spawnLoc != null)
        {
            mobMaybe = _stationSpawning.SpawnPlayerMob(
                spawnLoc.Value,
                jobId,
                character,
                station);
        }
        else
        {
            // Forced perma/sanitarium joins should not be redirected to cryosleep based on profile preference.
            mobMaybe = _stationSpawning.SpawnPlayerCharacterOnStation(station, jobId, forcedSpawnProfile);
        }

        DebugTools.AssertNotNull(mobMaybe);
        var mob = mobMaybe!.Value;

        // Inpatients should always receive a straightjacket, regardless of spawn path.
        if (inpatient)
        {
            CuffAndInjectWithKetamine(mob);
        }

        var brigTime = _permaBrigManager.GetBrigTime(player.UserId);
        var expireTime = TimeSpan.FromMinutes(brigTime) + Timing.CurTime;
        if (_inventory.TryGetSlotEntity(mob, "id", out var idUid))
        {
            var cardId = idUid.Value;
            if (TryComp<GenpopIdCardComponent>(cardId, out var card))
            {
                card.Crime = Loc.GetString("perma-prisoner-crime");
                card.SentenceDuration = TimeSpan.FromMinutes(brigTime);
                if (TryComp<ExpireIdCardComponent>(cardId, out var expire))
                {
                    expire.ExpireChannel = "Security";
                    expire.ExpireMessage = "perma-prisoner-release";
                }
                Dirty(cardId, card);
            }
            _idCard.SetExpireTime(cardId, expireTime);
        }
        AddComp(mob, new PrisonerComponent { PermaBrigSentenceExpireTime = expireTime });

        _mind.TransferTo(newMind, mob);
        _admin.UpdatePlayerList(player);

        _roles.MindAddJobRole(newMind, silent: false, jobPrototype: jobId);

        var briefing = Loc.GetString("perma-prisoner-briefing",
            ("minutes", brigTime));

        _audio.PlayGlobal(_lockUpSound, player);
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", briefing));
        _chat.ChatMessageToOne(ChatChannel.Server,
            briefing,
            wrappedMessage,
            default,
            false,
            player.Channel,
            Color.Red);

        _admin.UpdatePlayerList(player);

        var aev = new PlayerSpawnCompleteEvent(mob,
            player,
            jobId,
            false,
            true,
            0,
            station,
            character);

        _stationRecords.OnPlayerSpawn(aev);
        _trait.ApplyTraits(mob, character);
        _cryoSicknessSystem.ApplyComponent(mob);
    }

    private void CuffAndInjectWithKetamine(EntityUid prisoner)
    {
        var cuffs = _ent.SpawnEntity("ClothingOuterStraightjacket", Transform(prisoner).Coordinates);
        var cuffableComp = EnsureComp<CuffableComponent>(prisoner);
        _cuffableSystem.TryAddNewCuffs(prisoner, prisoner, cuffs, cuffableComp);
        if (!TryComp<BloodstreamComponent>(prisoner, out var bloodstream)) return;
        if (!_solutionContainerSystem.ResolveSolution(prisoner, bloodstream.ChemicalSolutionName, ref bloodstream.ChemicalSolution))
            return;

        _solutionContainerSystem.TryAddReagent(bloodstream.ChemicalSolution.Value, new ReagentId(_ketamine, null), _amountToInject, out _);
    }

    // private void OnRoundEnd(RoundEndMessageEvent ev) Auto decrease of perma sentence not yet implemented
    // {
    //
    // }
}
