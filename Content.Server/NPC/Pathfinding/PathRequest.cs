// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Threading;
using System.Threading.Tasks;
using Robust.Shared.Map;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Server.NPC.Pathfinding;

/// <summary>
/// Stores the in-progress data of a pathfinding request.
/// </summary>
public abstract class PathRequest
{
    public EntityUid Owner;
    public EntityCoordinates Start;

    public Task<PathResult> Task => Tcs.Task;
    public readonly TaskCompletionSource<PathResult> Tcs;

    public List<PathPoly> Polys = new();

    public bool Started = false;

    #region Pathfinding state

    public readonly Stopwatch Stopwatch = new();
    public PriorityQueue<ValueTuple<float, PathPoly>> Frontier = default!;
    public readonly Dictionary<PathPoly, float> CostSoFar = new();
    public readonly Dictionary<PathPoly, PathPoly> CameFrom = new();

    #endregion

    #region Data

    public readonly PathFlags Flags;
    public readonly int CollisionLayer;
    public readonly int CollisionMask;

    #endregion

    public PathRequest(EntityUid owner, EntityCoordinates start, PathFlags flags, int layer, int mask, CancellationToken cancelToken)
    {
        Owner = owner;
        Start = start;
        Flags = flags;
        CollisionLayer = layer;
        CollisionMask = mask;
        Tcs = new TaskCompletionSource<PathResult>(cancelToken);
    }
}

public sealed class AStarPathRequest : PathRequest
{
    public EntityCoordinates End;

    /// <summary>
    /// How close we need to be to the end node to be considered as arrived.
    /// </summary>
    public float Distance;

    public AStarPathRequest(
        EntityUid owner,
        EntityCoordinates start,
        EntityCoordinates end,
        PathFlags flags,
        float distance,
        int layer,
        int mask,
        CancellationToken cancelToken
    ) : base(owner, start, flags, layer, mask, cancelToken)
    {
        Distance = distance;
        End = end;
        Owner = owner;
    }
}

public sealed class BFSPathRequest : PathRequest
{
    /// <summary>
    /// How far away we're allowed to expand in distance.
    /// </summary>
    public float ExpansionRange;

    /// <summary>
    /// How many nodes we're allowed to expand
    /// </summary>
    public int ExpansionLimit;

    public BFSPathRequest(
        EntityUid owner,
        float expansionRange,
        int expansionLimit,
        EntityCoordinates start,
        PathFlags flags,
        int layer,
        int mask,
        CancellationToken cancelToken) : base(owner, start, flags, layer, mask, cancelToken)
        {
            ExpansionRange = expansionRange;
            ExpansionLimit = expansionLimit;
        }
}

/// <summary>
/// Stores the final result of a pathfinding request
/// </summary>
public sealed class PathResultEvent
{
    public PathResult Result;
    public readonly List<PathPoly> Path;

    public PathResultEvent(PathResult result, List<PathPoly> path)
    {
        Result = result;
        Path = path;
    }
}
