using System.Threading.Tasks;
using Robust.Shared.Network;

namespace Content.Server.Database;

public partial interface IServerDbManager
{
    Task<int> GetPermaRoundsLeft(NetUserId userId);
    Task SetPermaRoundsLeft(NetUserId userId, int brigSentence);
    Task<int> ModifyPermaRoundsLeft(NetUserId userId, int brigSentence);
    Task<int> GetPermaTimeLeft(NetUserId userId);
    Task SetPermaTimeLeft(NetUserId userId, int minutes);
    Task<int> ModifyPermaTimeLeft(NetUserId userId, int minutes);
    Task<int> GetPPpoints(NetUserId userId);
    Task SetPPpoints(NetUserId userId, int brigSentence);
    Task<int> ModifyPPpoints(NetUserId userId, int brigSentence);
}

public partial class ServerDbManager
{
    public Task<int> GetPermaRoundsLeft(NetUserId userId)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetPermaRoundsLeft(userId));
    }

    public Task SetPermaRoundsLeft(NetUserId userId, int permaSentence)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.SetPermaRoundsLeft(userId, permaSentence));
    }

    public Task<int> ModifyPermaRoundsLeft(NetUserId userId, int permaSentence)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ModifyPermaRoundsLeft(userId, permaSentence));
    }

    public Task<int> GetPermaTimeLeft(NetUserId userId)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetPermaTimeLeft(userId));
    }

    public Task SetPermaTimeLeft(NetUserId userId, int minutes)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.SetPermaTimeLeft(userId, minutes));
    }

    public Task<int> ModifyPermaTimeLeft(NetUserId userId, int minutes)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ModifyPermaTimeLeft(userId, minutes));
    }

    public Task<int> GetPPpoints(NetUserId userId)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.GetPPpoints(userId));
    }

    public Task SetPPpoints(NetUserId userId, int permaSentence)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.SetPPpoints(userId, permaSentence));
    }

    public Task<int> ModifyPPpoints(NetUserId userId, int permaSentence)
    {
        DbReadOpsMetric.Inc();
        return RunDbCommand(() => _db.ModifyPPpoints(userId, permaSentence));
    }
}
