using System.Threading.Tasks;
using Content.Server.Database;
using Content.Shared.Administration.Logs;
using Content.Shared.Database;
using Robust.Shared.Asynchronous;

namespace Content.Ratbite.Server.Bank;

/// <summary>
/// Handles getting and setting values in database for bank. MONEY. MONEY.
/// </summary>
public sealed partial class BankManager
{
    [Dependency] private IServerDbManager _db = default!;
    [Dependency] private ITaskManager _task = default!;
    //[Dependency] private IEntityManager _entManager = default!;
    [Dependency] private ISharedAdminLogManager _log = default!;

    private readonly List<Task> _pendingSaveTasks = new();
    private ISawmill _sawmill = default!;

    public void Shutdown()
    {
        _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
    }

    public void Initialize()
    {
        _sawmill = Logger.GetSawmill("server_bank");
    }

    public int GetShitcoins(NetUserId userId)
    {
        return Task.Run(() => GetShitcoinsAsync(userId)).GetAwaiter().GetResult();
    }

    public int ModifyShitcoins(NetUserId userId, int amountDelta)
    {
        var currentCoints = GetShitcoins(userId);
        var newShitcoins = currentCoints + amountDelta;
        if (newShitcoins < 0 || newShitcoins > 100000)
        {
            var shitcoinChange = (newShitcoins < 0) ? 0 : 100000;
            SetShitcoins(userId, shitcoinChange);
            return shitcoinChange - currentCoints;
        }

        return Task.Run(() => ModifyShitcoinsAsync(userId, amountDelta)).GetAwaiter().GetResult();
    }

    public int SetShitcoins(NetUserId userId, int coins)
    {
        var oldCoins = Task.Run(() => SetShitcoinsAsync(userId, coins)).GetAwaiter().GetResult();
        _log.Add(LogType.StorePurchase, LogImpact.Medium, $"Setting {userId} account to {coins} coins from {oldCoins}");
        return oldCoins;
    }

    private async Task<int> GetShitcoinsAsync(NetUserId userId) => await _db.GetShitcoins(userId);

    private async Task<int> ModifyShitcoinsAsync(NetUserId userId, int amountDelta)
    {
        var task = Task.Run(() => _db.ModifyShitcoins(userId, amountDelta));
        TrackPending(task);
        return await task;
    }

    private async Task<int> SetShitcoinsAsync(NetUserId userId, int amount)
    {
        // We need to block it first to ensure we don't read our own amount, hence sync function
        var oldAmount = GetShitcoins(userId);
        await SetShitcoinsAsyncInternal(userId, amount, oldAmount);
        return oldAmount;
    }

    private async Task SetShitcoinsAsyncInternal(NetUserId userId, int amount, int oldAmount)
    {
        var task = Task.Run(() => _db.SetShitcoins(userId, amount));
        TrackPending(task);
        await task;
    }

    /// <summary>
    /// Track a database save task to make sure we block server shutdown on it.
    /// </summary>
    private async void TrackPending(Task task)
    {
        _pendingSaveTasks.Add(task);

        try
        {
            await task;
        }
        finally
        {
            _pendingSaveTasks.Remove(task);
        }
    }
}
