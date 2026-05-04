using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Robust.Shared.Network;

namespace Content.Server.Database;
public abstract partial class ServerDbBase
{
    #region Perma Brig

    public async Task<int> GetPermaRoundsLeft(NetUserId userId) // Ratbite
    {
        await using var db = await GetDb();

        return await db.DbContext.Player
            .Where(dbPlayer => dbPlayer.UserId == userId)
            .Select(dbPlayer => dbPlayer.BrigSentence)
            .SingleOrDefaultAsync();
    }

    public async Task SetPermaRoundsLeft(NetUserId userId, int brigSentence) // Ratbite
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return;

        dbPlayer.BrigSentence = brigSentence;
        await db.DbContext.SaveChangesAsync();
    }

    [Obsolete]

    public async Task<int> ModifyPermaRoundsLeft(NetUserId userId, int brigSentence) // Goobstation
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return brigSentence;

        dbPlayer.BrigSentence += brigSentence;
        await db.DbContext.SaveChangesAsync();
        return dbPlayer.BrigSentence;
    }

    public async Task<int> GetPermaTimeLeft(NetUserId userId) // Ratbite
    {
        await using var db = await GetDb();

        return await db.DbContext.Player
            .Where(dbPlayer => dbPlayer.UserId == userId)
            .Select(dbPlayer => dbPlayer.BrigTime)
            .SingleOrDefaultAsync();
    }

    public async Task SetPermaTimeLeft(NetUserId userId, int minutes) // Ratbite
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return;

        dbPlayer.BrigTime = minutes;
        await db.DbContext.SaveChangesAsync();
    }

    public async Task<int> ModifyPermaTimeLeft(NetUserId userId, int minutes) // Goobstation
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return minutes;

        dbPlayer.BrigTime += minutes;
        await db.DbContext.SaveChangesAsync();
        return dbPlayer.BrigTime;
    }
    public async Task<int> GetPPpoints(NetUserId userId) // Ratbite
    {
        await using var db = await GetDb();

        return await db.DbContext.Player
            .Where(dbPlayer => dbPlayer.UserId == userId)
            .Select(dbPlayer => dbPlayer.PPpoints)
            .SingleOrDefaultAsync();
    }

    public async Task SetPPpoints(NetUserId userId, int pppoints) // Ratbite
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return;

        dbPlayer.PPpoints = pppoints;
        await db.DbContext.SaveChangesAsync();
    }

    public async Task<int> ModifyPPpoints(NetUserId userId, int pppoints) // Goobstation
    {
        await using var db = await GetDb();

        var dbPlayer = await db.DbContext.Player.Where(dbPlayer => dbPlayer.UserId == userId).SingleOrDefaultAsync();
        if (dbPlayer == null)
            return pppoints;

        dbPlayer.PPpoints += pppoints;
        await db.DbContext.SaveChangesAsync();
        return dbPlayer.PPpoints;
    }

    #endregion
}
