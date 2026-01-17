using Content.Server.Administration;
using Content.Server.Administration.Managers;
using Content.Server.Chat.Managers;
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
        public string Command => "perma:sentence";
        public string Description => "check your/another players Brig Sentence";
        public string Help => "Usage: perma:sentence <optional: player>"
                              + "\n    player: (optional) who to view brigsentence of.";
        public void Execute(IConsoleShell shell, string argStr, string[] args)
        {
            string balance;
            switch (args.Length)
            {
                case 0:
                    if(shell.Player is not { } player){
                        shell.WriteError(Loc.GetString("shell-cannot-run-command-from-server"));
                        break;
                    }

                    balance = Loc.GetString("perma-your-current-sentence",
                        ("sentence", _permaBrigManager.GetBrigSentence(shell.Player.UserId)));

                    _chatManager.ChatMessageToOne(ChatChannel.Local, balance, balance, EntityUid.Invalid, false, shell.Player.Channel);
                    shell.WriteLine(balance);

                    break;
                case 1:
                    if(shell.Player is { } player2)
                    {
                        var plyMgrm = IoCManager.Resolve<IPlayerManager>();
                        if (!plyMgrm.TryGetUserId(args[0], out var targetPlayerm))
                        {
                            shell.WriteError(Loc.GetString("perma-command-invalid-player"));
                            break;
                        }

                        if ((targetPlayerm != shell.Player.UserId)
                            && !_adminManager.HasAdminFlag(shell.Player, AdminFlags.ViewNotes, false))
                        {
                            Loc.GetString("perma-other-current-sentence-deny");
                            break;
                        }

                        balance = Loc.GetString("perma-other-current-sentence",
                            ("player", targetPlayerm.UserId),
                            ("sentence", _permaBrigManager.GetBrigSentence(targetPlayerm)));

                        _chatManager.ChatMessageToOne(ChatChannel.Local,
                            balance,
                            balance,
                            EntityUid.Invalid,
                            false,
                            shell.Player.Channel);

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
                        ("sentence", _permaBrigManager.GetBrigSentence(targetPlayer)));

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
        public string Description => "Add rounds to player's brig sentence";
        public string Help => "Usage: perma:brig <player> <rounds>"
                              + "\n    player: who to add time to."
                              + "\n    rounds: number of rounds to add to sentence.";
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

            if (!int.TryParse(args[1], out var roundCount))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-time"));
                return;
            }

            _permaBrigManager.AddBrigSentence(targetPlayer, roundCount);

            var message = Loc.GetString("perma-add-time-to-player",
                ("rounds", roundCount),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if(shell.Player is { } player)
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
                2 => CompletionResult.FromHint("<Rounds>"),
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
        public string Description => "Remove rounds from player's brig sentence";
        public string Help => "Usage: perma:pardon <player> <rounds>"
                              + "\n    player: who to remove time from."
                              + "\n    rounds: number of rounds to remove from sentence.";
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

            if (!int.TryParse(args[1], out var roundCount))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-time"));
                return;
            }

            _permaBrigManager.RemoveBrigSentence(targetPlayer, roundCount);

            var message = Loc.GetString("perma-rem-time-to-player",
                ("rounds", roundCount),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if(shell.Player is { } player)
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
                2 => CompletionResult.FromHint("<Rounds>"),
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
        public string Description => "Set the number rounds player is serving in brig";
        public string Help => "Usage: permaset <player> <rounds>"
                              + "\n    player: who to set time from."
                              + "\n    rounds: number of rounds to set perma time to.";
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

            if (!int.TryParse(args[1], out var roundCount))
            {
                shell.WriteError(Loc.GetString("perma-command-invalid-time"));
                return;
            }

            _permaBrigManager.SetBrigSentence(targetPlayer, roundCount);

            var message = Loc.GetString("perma-set-time-to-player",
                ("rounds", roundCount),
                ("player", targetPlayer.UserId));

            shell.WriteLine(message);

            if(shell.Player is { } player)
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
                2 => CompletionResult.FromHint("<Rounds>"),
                _ => CompletionResult.Empty
            };
        }
    }
}
