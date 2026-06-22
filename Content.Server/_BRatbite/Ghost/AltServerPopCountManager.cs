using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Content.Shared._BRatbite.CCVar;
using Robust.Shared.Configuration;
using Robust.Shared.Timing;

namespace Content.Server._BRatbite.Ghost;

public sealed partial class AltServerPopCountManager
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    private readonly HttpClient client = new HttpClient();

    private int _playerPop = 0;
    private string _serverIpAddress = "";
    private int _serverPort = 0;
    private TimeSpan _lastUpdate;
    private TimeSpan _popRefreshTime = TimeSpan.FromSeconds(60);
    private HttpClient _httpClient = new();
    public event Action<int>? PopUpdated;

    public int GetCachedPlayerPop()
    {
        return _playerPop;
    }

    public void Initialize()
    {
        _serverIpAddress = _cfg.GetCVar(RatbiteCVars.AltServerIP);
        _serverPort = _cfg.GetCVar(RatbiteCVars.AltServerPort);
        _popRefreshTime = TimeSpan.FromSeconds(_cfg.GetCVar(RatbiteCVars.PopCountRefreshSeconds));
        _cfg.OnValueChanged(RatbiteCVars.AltServerIP, value => _serverIpAddress = value);
        _cfg.OnValueChanged(RatbiteCVars.AltServerPort, value => _serverPort = value);
        _cfg.OnValueChanged(RatbiteCVars.PopCountRefreshSeconds, value => _popRefreshTime = TimeSpan.FromSeconds(value));
    }

    public void Update()
    {
        if (_serverIpAddress.Length == 0) return;
        if (_lastUpdate + _popRefreshTime < _timing.RealTime)
        {
            _lastUpdate = _timing.RealTime;
            _ = UpdatePopCount();
        }
    }
    private record struct ServerStatus(int players);
    private async Task UpdatePopCount()
    {
        var serverIp = $"http://{_serverIpAddress}:{_serverPort}/status";
        var response = await _httpClient.GetFromJsonAsync<ServerStatus>(serverIp);
        var oldPop = _playerPop;
        _playerPop = response.players;
        if (oldPop != _playerPop)
            PopUpdated?.Invoke(_playerPop);
    }
}

