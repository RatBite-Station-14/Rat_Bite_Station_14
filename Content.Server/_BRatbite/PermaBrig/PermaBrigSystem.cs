using Content.Server.Administration.Systems;
using Content.Server.Chat.Managers;
using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Players.PlayTimeTracking;
using Content.Server.Spawners.Components;
using Content.Server.Station.Systems;
using Content.Shared.Chat;
using Content.Shared.GameTicking;
using Content.Shared.Mind;
using Content.Shared.Players;
using Content.Shared.Roles;
using Content.Shared.Roles.Jobs;
using Robust.Server.Audio;
using Robust.Server.Player;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;

namespace Content.Server._BRatbite.PermaBrig;

/// <summary>
/// This handles...
/// </summary>
public sealed class PermaBrigSystem : GameRuleSystem<PermaBrigComponent>
{
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly IRobustRandom _robustRandom = default!;
    [Dependency] private readonly IPlayerManager _player = default!;
    [Dependency] private readonly IChatManager _chat = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _ticker = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PlayTimeTrackingSystem _playTimeTrackings = default!;
    [Dependency] private readonly StationSpawningSystem _stationSpawning = default!;
    [Dependency] private readonly AdminSystem _admin = default!;
    [Dependency] private readonly SharedJobSystem _jobs = default!;
    [Dependency] private readonly SharedRoleSystem _roles = default!;
    [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
    [Dependency] private readonly AudioSystem _audio = default!;

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
            if (_permaBrigManager.GetBrigSentence(session.UserId) == 0)
                continue;
            PermaIndividuals.Add(session);
            _permaBrigManager.RemoveBrigSentence(session.UserId, 1);
            _sawmill.Info($"Player intercepted for perma: {session}");
        }

        foreach (var player in PermaIndividuals)
        {
            pool.Remove(player);
            GameTicker.PlayerJoinGame(player);

            SpawnPrisonerPlayer(player);

            _sawmill.Info($"Player sent to perma: {player}");
        }
    }

    private void OnPlayerBeforeSpawning(PlayerBeforeSpawnEvent ev)
    {
        if (!_ticker.IsGameRuleActive<PermaBrigComponent>())
            return;

        if (!ev.LateJoin) //OnPlayerSpawning handles the start round spawning, before traitor picking, so this just needs to handle late joiners.
            return;

        if (_permaBrigManager.GetBrigSentence(ev.Player.UserId) == 0)
            return;

        PermaIndividuals.Add(ev.Player);
        _permaBrigManager.RemoveBrigSentence(ev.Player.UserId, 1);

        SpawnPrisonerPlayer(ev.Player);

        ev.Handled = true;

        _sawmill.Info($"Player sent to perma: {ev.Player}");
    }

    private EntityCoordinates? GetSpawnLocation()
    {
        var points = EntityQueryEnumerator<SpawnPointComponent, TransformComponent>();
        var possiblePositions = new List<EntityCoordinates>();

        while ( points.MoveNext(out var uid, out var spawnPoint, out var xform))
        {
            if (spawnPoint.SpawnType == SpawnPointType.Job&&
                (spawnPoint.Job == "Prisoner"))
            {
                possiblePositions.Add(xform.Coordinates);
            }
        }

        if (possiblePositions.Count == 0)
            return null;

        return _random.Pick(possiblePositions);
    }

    private void SpawnPrisonerPlayer(ICommonSession player)
    {
        var stations = _ticker.GetSpawnableStations();
        _robustRandom.Shuffle(stations);
        var station = EntityUid.Invalid;
        if (stations.Count != 0)
            station = stations[0];

        var character = _ticker.GetPlayerProfile(player);

        var data = player.ContentData();

        var newMind = _mind.CreateMind(data!.UserId, character.Name);
        _mind.SetUserId(newMind, data.UserId);

        var jobPrototype = _prototypeManager.Index<JobPrototype>("Prisoner");

        _playTimeTrackings.PlayerRolesChanged(player);

        GetSpawnLocation();

        EntityCoordinates? spawnLoc = null;
        EntityUid? mobMaybe = null;

        spawnLoc = GetSpawnLocation();

        if (spawnLoc!=null)
        {
            mobMaybe = _stationSpawning.SpawnPlayerMob(
                spawnLoc.Value,
                "Prisoner",
                character,
                station);
        }
        else
        {
            mobMaybe = _stationSpawning.SpawnPlayerCharacterOnStation(station, "Prisoner", character);
        }

        DebugTools.AssertNotNull(mobMaybe);
        var mob = mobMaybe!.Value;

        _mind.TransferTo(newMind, mob);
        _admin.UpdatePlayerList(player);

        _roles.MindAddJobRole(newMind, silent: false, jobPrototype:"Prisoner");

        var briefing = Loc.GetString("perma-prisoner-briefing",
            ("rounds", _permaBrigManager.GetBrigSentence(player.UserId)));

        _audio.PlayGlobal(_lockUpSound, player);
        var wrappedMessage = Loc.GetString("chat-manager-server-wrap-message", ("message", briefing));
        _chat.ChatMessageToOne(ChatChannel.Server, briefing, wrappedMessage, default, false, player.Channel,
            Color.Red);

        _admin.UpdatePlayerList(player);
    }

    // private void OnRoundEnd(RoundEndMessageEvent ev) Auto decrease of perma sentence not yet implemented
    // {
    //
    // }
}
