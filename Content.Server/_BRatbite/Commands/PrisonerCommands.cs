// SPDX-FileCopyrightText: 2022 Júlio César Ueti <52474532+Mirino97@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 Kara <lunarautomaton6@gmail.com>
// SPDX-FileCopyrightText: 2022 ShadowCommander <10494922+ShadowCommander@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Chief-Engineer <119664036+Chief-Engineer@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 Pieter-Jan Briers <pieterjan.briers+git@gmail.com>
// SPDX-FileCopyrightText: 2023 Riggle <27156122+RigglePrime@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 metalgearsloth <31366439+metalgearsloth@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Piras314 <p1r4s@proton.me>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Server.Administration.Managers;
using Content.Shared.Administration;
using Content.Shared.CCVar;
using Content.Server.Database;
using Content.Shared.Database;
using Content.Shared.Roles;
using Robust.Shared.Configuration;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Prototypes;
using System.Linq;
using Content.Server.Players.JobWhitelist;

namespace Content.Server.Administration.Commands;

[AdminCommand(AdminFlags.Ban)]
public sealed class PermaBrigCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly JobWhitelistManager _jobWhitelist = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IBanManager _bans = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IConfigurationManager _cfg = default!;

    public override string Command => "oldpermabrig";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 1),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        var player = args[0].Trim();
        var prisonerJob = new ProtoId<JobPrototype>("Prisoner");

        var data = await _playerLocator.LookupIdByNameAsync(player);
        if (data != null)
        {
            var guid = data.UserId;
            var isWhitelisted = await _db.IsJobWhitelisted(guid, prisonerJob);
            if (isWhitelisted)
            {
                shell.WriteLine(Loc.GetString("cmd-permabrig-player-already-imprisoned",
                    ("player", player)));
                return;
            }
            if (!Enum.TryParse(_cfg.GetCVar(CCVars.ImprisonmentDefaultSeverity), out NoteSeverity severity))
            {
                Logger.WarningS("admin.role_ban", "Imprisonment roleban severity could not be parsed from config! Defaulting to medium.");
                severity = NoteSeverity.Medium;
            }
            var now = DateTimeOffset.UtcNow; // this groups bans together
            foreach (var proto in _prototypes.EnumeratePrototypes<JobPrototype>().Where(value => value.ID != "Prisoner"))
            {
                _bans.CreateRoleBan(guid, data.Username, shell.Player?.UserId, null, data.LastHWId, proto.ID, 0, severity, "cmd-permabrig-ban-description", now);
            }
            _jobWhitelist.AddWhitelist(guid, prisonerJob);

            shell.WriteLine(Loc.GetString("cmd-permabrig-player-imprisoned-successfully",
                ("player", player)));
            return;
        }

        shell.WriteError(Loc.GetString("cmd-permabrig-player-not-found", ("player", player)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                Loc.GetString("cmd-permabrig-hint-player"));
        }

        return CompletionResult.Empty;
    }
}

[AdminCommand(AdminFlags.Ban)]
public sealed class PermaPardonCommand : LocalizedCommands
{
    [Dependency] private readonly IServerDbManager _db = default!;
    [Dependency] private readonly JobWhitelistManager _jobWhitelist = default!;
    [Dependency] private readonly IPlayerLocator _playerLocator = default!;
    [Dependency] private readonly IPlayerManager _players = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly IBanManager _bans = default!;

    public override string Command => "oldpermapardon";

    public override async void Execute(IConsoleShell shell, string argStr, string[] args) //_bans.PardonRoleBanByDescription("Rolebanned to imprison. Do not create bans with this exact description for they will get deleted.", data.UserId,)
    {
        if (args.Length != 1)
        {
            shell.WriteError(Loc.GetString("shell-wrong-arguments-number-need-specific",
                ("properAmount", 1),
                ("currentAmount", args.Length)));
            shell.WriteLine(Help);
            return;
        }

        var player = args[0].Trim();
        var prisonerJob = new ProtoId<JobPrototype>("Prisoner");

        var data = await _playerLocator.LookupIdByNameAsync(player);
        if (data != null)
        {
            var guid = data.UserId;
            var isWhitelisted = await _db.IsJobWhitelisted(guid, prisonerJob);
            if (!isWhitelisted)
            {
                shell.WriteError(Loc.GetString("cmd-permabrig-player-not-imprisoned",
                    ("player", player)));
                return;
            }
            _jobWhitelist.RemoveWhitelist(guid, prisonerJob);
            await _bans.PardonRoleBanByDescription("cmd-permabrig-ban-description", guid, shell.Player?.UserId);
            return;
        }

        shell.WriteError(Loc.GetString("cmd-permabrig-player-not-found", ("player", player)));
    }

    public override CompletionResult GetCompletion(IConsoleShell shell, string[] args)
    {
        if (args.Length == 1)
        {
            return CompletionResult.FromHintOptions(
                _players.Sessions.Select(s => s.Name),
                Loc.GetString("cmd-permabrig-hint-player"));
        }

        return CompletionResult.Empty;
    }
}
