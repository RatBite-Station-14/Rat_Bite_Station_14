using System.Threading.Tasks;
using Content.Server.Database;
using Robust.Shared.Asynchronous;
using Robust.Shared.Network;

namespace Content.Server._BRatbite.PermaBrig
{

    /// <summary>
    /// Handles getting and setting values in database for perma sentences
    /// Modified version of GoobStations ServerCurrencyManager
    /// </summary>

    public sealed class PermaBrigManager
    {
        [Dependency] private readonly IServerDbManager _db = default!;
        [Dependency] private readonly ITaskManager _task = default!;
        private readonly List<Task> _pendingSaveTasks = new();

        public void Shutdown()
        {
            _task.BlockWaitOnTask(Task.WhenAll(_pendingSaveTasks));
        }

        private ISawmill _sawmill = default!;

        public void Initialize()
        {
            _sawmill = Logger.GetSawmill("server_permabrig");
        }

        /// <summary>
        /// Adds perma rounds to a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="rounds">The number of rounds to add in perma.</param>
        /// <returns>An integer containing the new total of rounds to serve.</returns>
        public int AddBrigSentence(NetUserId userId, int rounds)
        {
            var newTotal = ModifyBrigSentence(userId, rounds);
            _sawmill.Info($"Added {rounds} rounds to {userId} sentence. Current sentence: {newTotal}");
            return newTotal;
        }

        /// <summary>
        /// Removes perma rounds from a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="rounds">The number of rounds to remove in perma.</param>
        /// <returns>An integer containing the new total of rounds to serve.</returns>
        public int RemoveBrigSentence(NetUserId userId, int rounds)
        {
            var newTotal = ModifyBrigSentence(userId, -rounds);
            _sawmill.Info($"Removed {rounds} rounds from {userId} sentence. Current sentence: {newTotal}");
            return newTotal;
        }

        /// <summary>
        /// Sets perma rounds for player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="rounds">The number of rounds to spend in perma</param>
        /// <returns>An integer containing the old sentence attributed to the player.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBrigSentence(NetUserId)"/> prior to this.</remarks>
        public int SetBrigSentence(NetUserId userId, int rounds)
        {
            var oldSentence = Task.Run(() => SetBrigSentenceAsync(userId, rounds)).GetAwaiter().GetResult();
            _sawmill.Info($"Setting {userId} sentence to {rounds} rounds from {oldSentence}");
            return oldSentence;
        }

        /// <summary>
        /// Gets a player's perma sentence.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>The players current sentence.</returns>
        public int GetBrigSentence(NetUserId userId)
        {
            return Task.Run(() => GetBrigSentenceAsync(userId)).GetAwaiter().GetResult();
        }

        /// <summary>
        /// Adds PPpoints to a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of PPpoints to add.</param>
        /// <returns>An integer containing the new amount of PPpoints attributed to the player.</returns>
        public int AddPPpoints(NetUserId userId, int amount)
        {
            var newAmount = ModifyPPpoints(userId, amount);
            _sawmill.Info($"Added {amount} PPpoints to {userId} account. Current PPpoint total: {newAmount}");
            return newAmount;
        }

        /// <summary>
        /// Removes PPpoints from a player.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of PPpoints to remove.</param>
        /// <returns>An integer containing the old amount of PPpoints attributed to the player.</returns>
        public int RemovePPpoints(NetUserId userId, int amount)
        {
            var oldAmount = ModifyPPpoints(userId, -amount);
            _sawmill.Info($"Removed {amount} PPpoints from {userId} account. Previous PPpoint total: {oldAmount}");
            return oldAmount;
        }

        /// <summary>
        /// Sets a player's PPpoint total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The amount of PPpoints that will be set.</param>
        /// <returns>An integer containing the old amount of PPpoints attributed to the player.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetPPpoints(NetUserId)"/> prior to this.</remarks>
        public int SetPPpoints(NetUserId userId, int amount)
        {
            var oldAmount = Task.Run(() => SetPPpointsAsync(userId, amount)).GetAwaiter().GetResult();
            _sawmill.Info($"Setting {userId} PPpoint total to {amount} from {oldAmount}");
            return oldAmount;
        }

        /// <summary>
        /// Gets a player's PPpoint total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>The players PPpoint total.</returns>
        public int GetPPpoints(NetUserId userId)
        {
            return Task.Run(() => GetPPpoints(userId)).GetAwaiter().GetResult();
        }

        #region INTERNAL/ASYNC

        /// <summary>
        /// Modifies a player's brig sentence.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The rounds in perma that will to add.</param>
        /// <returns>An integer containing the additional rounds in perma.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBrigSentence(NetUserId)"/> after to this.</remarks>
        private int ModifyBrigSentence(NetUserId userId, int amountDelta)
        {
            var result = Task.Run(() => ModifyBrigSentenceAsync(userId, amountDelta)).GetAwaiter().GetResult();
            if (result < 0)
            {
                SetBrigSentence(userId, 0);
                result = 0;
            }
            return result;
        }

        /// <summary>
        /// Sets a player's brig sentence.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The number of rounds in perma to set.</param>
        /// <param name="oldAmount">The number of rounds originally set.</param>
        /// <remarks>This and its classes will block server shutdown until execution finishes.</remarks>
        private async Task SetBrigSentenceAsyncInternal(NetUserId userId, int amount, int oldAmount)
        {
            var task = Task.Run(() => _db.SetPermaRoundsLeft(userId, amount));
            TrackPending(task);
            await task;
        }

        /// <summary>
        /// Sets a player's brig sentence.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">he number of rounds in perma to set.</param>
        /// <returns>The number of rounds originally set.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetBrigSentence(NetUserId)"/> prior to this.</remarks>
        private async Task<int> SetBrigSentenceAsync(NetUserId userId, int amount)
        {
            // We need to block it first to ensure we don't read our own amount, hence sync function
            var oldAmount = GetBrigSentence(userId);
            await SetBrigSentenceAsyncInternal(userId, amount, oldAmount);
            return oldAmount;
        }

        /// <summary>
        /// Gets the number of rounds a player needs to serve in perma.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>An integer containing the rounds left to serve.</returns>
        private async Task<int> GetBrigSentenceAsync(NetUserId userId) => await _db.GetPermaRoundsLeft(userId);

        /// <summary>
        /// Modifies a player's sentence.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The rounds in perma that will to add.</param>
        /// <returns>An integer containing the additional rounds in perma.</returns>
        /// <remarks>This and its classes will block server shutdown until execution finishes.</remarks>
        private async Task<int> ModifyBrigSentenceAsync(NetUserId userId, int amountDelta)
        {
            var task = Task.Run(() => _db.ModifyPermaRoundsLeft(userId, amountDelta));
            TrackPending(task);
            return await task;
        }

        /// <summary>
        /// Modifies a player's PPpoints total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The PPpoints to add.</param>
        /// <returns>An integer containing the new PPpoints.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetPPpoints(NetUserId)"/> after to this.</remarks>
        private int ModifyPPpoints(NetUserId userId, int amountDelta)
        {
            var result = Task.Run(() => ModifyPPpointsAsync(userId, amountDelta)).GetAwaiter().GetResult();
            return result;
        }

        /// <summary>
        /// Sets a player's PPpoints total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">The number of PPpoints to set.</param>
        /// <param name="oldAmount">The number of PPpoints originally set.</param>
        /// <remarks>This and its classes will block server shutdown until execution finishes.</remarks>
        private async Task SetPPpointsAsyncInternal(NetUserId userId, int amount, int oldAmount)
        {
            var task = Task.Run(() => _db.SetPPpoints(userId, amount));
            TrackPending(task);
            await task;
        }

        /// <summary>
        /// Sets a player's PPpoints total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amount">he number of PPpoints set.</param>
        /// <returns>The number of PPpoints originally set.</returns>
        /// <remarks>Use the return value instead of calling <see cref="GetPPpoints(NetUserId)"/> prior to this.</remarks>
        private async Task<int> SetPPpointsAsync(NetUserId userId, int amount)
        {
            // We need to block it first to ensure we don't read our own amount, hence sync function
            var oldAmount = GetPPpoints(userId);
            await SetPPpointsAsyncInternal(userId, amount, oldAmount);
            return oldAmount;
        }

        /// <summary>
        /// Gets the number of PPpoints a player has.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <returns>An integer containing the PPpoints total.</returns>
        private async Task<int> GetPPpointsAsync(NetUserId userId) => await _db.GetPPpoints(userId);

        /// <summary>
        /// Modifies a player's PPpoints total.
        /// </summary>
        /// <param name="userId">The player's NetUserId</param>
        /// <param name="amountDelta">The amount of PPpoints that will be given or taken.</param>
        /// <returns>An integer containing the new amount of PPpoints attributed to the player.</returns>
        /// <remarks>This and its classes will block server shutdown until execution finishes.</remarks>
        private async Task<int> ModifyPPpointsAsync(NetUserId userId, int amountDelta)
        {
            var task = Task.Run(() => _db.ModifyPPpoints(userId, amountDelta));
            TrackPending(task);
            return await task;
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

        #endregion
    }
}
