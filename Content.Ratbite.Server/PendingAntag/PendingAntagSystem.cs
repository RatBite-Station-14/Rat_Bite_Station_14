// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Antag;
using Content.Server.Antag.Components;
using Content.Shared.Antag;
using Content.Shared.GameTicking;
using Content.Shared.Roles;

namespace Content.Ratbite.Server.PendingAntag;

public sealed partial class PendingAntagSystem : EntitySystem
{
    [Dependency] private IPrototypeManager _prototypeManager = default!;
    [Dependency] private AntagSelectionSystem _selection = default!;

    public Dictionary<NetUserId, (ProtoId<AntagSpecifierPrototype>, Entity<AntagSelectionComponent>)> PendingAntags = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundRestartCleanupEvent>(OnRoundRestart);
        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawned);
    }

    private void OnPlayerSpawned(PlayerSpawnCompleteEvent ev)
    {
        if (ev.LateJoin)
            return;

        if (ev.JobId == null || !_prototypeManager.Index<JobPrototype>(ev.JobId).CanBeAntag)
            return;

        if (!PendingAntags.Remove(ev.Player.UserId, out var pendingAntag))
            return;

        _selection.TryMakeAntag(pendingAntag.Item2, pendingAntag.Item1, ev.Player, true);
    }

    private void OnRoundRestart(RoundRestartCleanupEvent ev)
    {
        PendingAntags.Clear();
    }
}
