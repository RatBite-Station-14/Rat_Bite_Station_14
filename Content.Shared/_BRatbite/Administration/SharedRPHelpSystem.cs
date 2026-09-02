using Content.Shared.Mind;
using Robust.Shared.Player;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.Administration;

public abstract class SharedRPHelpSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly ISharedPlayerManager _playerManager = default!;
    public override void Initialize()
    {
        base.Initialize();
    }
}

