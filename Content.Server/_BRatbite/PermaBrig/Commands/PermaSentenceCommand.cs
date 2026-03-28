using Content.Server.Administration;
using Content.Server.Administration.Commands;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
using Content.Server.Commands;
using Content.Server.Players.PlayTimeTracking;
using Content.Shared.Administration;
using Content.Shared.Chat;
using Robust.Server.Player;
using Robust.Shared.Console;
using Robust.Shared.Player;

namespace Content.Server._BRatbite.PermaBrig.Commands
{
    [AnyCommand]
    public sealed class PermaSentenceCommand : IConsoleCommand
    {
        [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        [Dependency] private readonly IAdminManager _adminManager = default!;
        [Dependency] private readonly PlayTimeTrackingManager _tracking = default!;
        public string Command => "perma:sentence";
        public string Description => "check your/another players Brig Sentence";

        public string Help => "Usage: perma:sentence <optional: player>"
                              + "\n    player: (optional) who to view brigsentence of.";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            string balance;
            var commonSession = shell.Player;
            if (commonSession != null)
            {
                _tracking.QueueRefreshTrackers(commonSession);
            }

            switch (args.Length)
            {
                case 0:
                    if (commonSession is not { } player)
                    {
                        shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                        break;
                    }

                    balance = Loc.GetString("perma-your-current-sentence",
                        ("sentence", _permaBrigManager.GetBrigTimeLabel(commonSession.UserId)));

                    _chatManager.ChatMessageToOne(ChatChannel.Local,
                        balance,
                        balance,
                        EntityUid.Invalid,
                        false,
                        commonSession.Channel);
                    shell.WriteLine(balance);

                    break;
                case 1:
                    if (commonSession is { } player2)
                    {
                        var plyMgrm = IoCManager.Resolve<IPlayerManager>();
                        if (!plyMgrm.TryGetUserId(args[0], out var targetPlayerm))
                        {
                            shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                            break;
                        }

                        if ((targetPlayerm != commonSession.UserId)
                            && !_adminManager.HasAdminFlag(commonSession, AdminFlags.ViewNotes, false))
                        {
                            Loc.GetString("perma-other-current-sentence-deny");
                            break;
                        }

                        balance = Loc.GetString("perma-other-current-sentence",
                            ("player", targetPlayerm.UserId),
                            ("sentence", _permaBrigManager.GetBrigTimeLabel(targetPlayerm)));

                        _chatManager.ChatMessageToOne(ChatChannel.Local,
                            balance,
                            balance,
                            EntityUid.Invalid,
                            false,
                            commonSession.Channel);

                        shell.WriteLine(balance);

                        break;
                    }

                    var plyMgr = IoCManager.Resolve<IPlayerManager>();
                    if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
                    {
                        shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                        break;
                    }

                    balance = Loc.GetString("perma-other-current-sentence",
                        ("player", targetPlayer.UserId),
                        ("sentence", _permaBrigManager.GetBrigTimeLabel(targetPlayer)));

                    shell.WriteLine(balance);

                    break;
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<player> (optional)"),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Ban)]
    public sealed class PermaSentenceAddCommand : IConsoleCommand
    {
        [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        public string Command => "perma:brig";
        public string Description => "Add time to player's brig sentence";

        public string Help => "Usage: perma:brig <player> <rounds>"
                              + "\n    player: who to add time to."
                              + "\n    time: time to add to sentence. (ex: 1h20m is an hour and 20 minutes)";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                return;
            }

            var minutes = PlayTimeCommandUtilities.CountMinutes(args[1]);

            _permaBrigManager.AddBrigTime(targetPlayer, minutes);

            var message = Loc.GetString("perma-add-time-to-player",
                ("minutes", minutes),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if (shell.Player is { } player)
            {
                _chatManager.ChatMessageToOne(ChatChannel.Local,
                    message,
                    message,
                    EntityUid.Invalid,
                    false,
                    shell.Player.Channel);
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<Player>"),
                2 => CompletionResult.FromHint("<Time>"),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Ban)]
    public sealed class PermaSentenceRemoveCommand : IConsoleCommand
    {
        [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        public string Command => "perma:pardon";
        public string Description => "Remove time from player's brig sentence";

        public string Help => "Usage: perma:pardon <player> <rounds>"
                              + "\n    player: who to remove time from."
                              + "\n    time: time to remove from sentence. (ex: 1h20m is an hour and 20 minutes)";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                return;
            }

            var minutes = PlayTimeCommandUtilities.CountMinutes(args[1]);
            _permaBrigManager.RemoveBrigTime(targetPlayer, minutes);

            var message = Loc.GetString("perma-rem-time-to-player",
                ("minutes", minutes),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if (shell.Player is { } player)
            {
                _chatManager.ChatMessageToOne(ChatChannel.Local,
                    message,
                    message,
                    EntityUid.Invalid,
                    false,
                    shell.Player.Channel);
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<Player>"),
                2 => CompletionResult.FromHint("<Time>"),
                _ => CompletionResult.Empty
            };
        }
    }

    [AdminCommand(AdminFlags.Ban)]
    public sealed class PermaSentenceSetCommand : IConsoleCommand
    {
        [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
        [Dependency] private readonly IChatManager _chatManager = default!;
        public string Command => "perma:set";
        public string Description => "Set the time player is serving in brig";

        public string Help => "Usage: permaset <player> <rounds>"
                              + "\n    player: who to set time from."
                              + "\n    time: time to set the sentence to. (1h20m is an hour and 20 minutes)";

        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            if (args.Length != 2)
            {
                return;
            }

            var plyMgr = IoCManager.Resolve<IPlayerManager>();
            if (!plyMgr.TryGetUserId(args[0], out var targetPlayer))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                return;
            }

            var minutes = PlayTimeCommandUtilities.CountMinutes(args[1]);

            _permaBrigManager.SetBrigTime(targetPlayer, minutes);

            var message = Loc.GetString("perma-set-time-to-player",
                ("minutes", minutes),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if (shell.Player is { } player)
            {
                _chatManager.ChatMessageToOne(ChatChannel.Local,
                    message,
                    message,
                    EntityUid.Invalid,
                    false,
                    shell.Player.Channel);
            }
        }

        public CompletionResult GetCompletion(IConsoleShell shell, string[] args)
        {
            return args.Length switch
            {
                1 => CompletionResult.FromHintOptions(CompletionHelper.SessionNames(), "<Player>"),
                2 => CompletionResult.FromHint("<Time>"),
                _ => CompletionResult.Empty
            };
        }
    }
}
