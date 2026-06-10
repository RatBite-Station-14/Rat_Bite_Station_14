using Content.Server.Administration;
using Content.Shared.Administration;
using Robust.Shared.Player;
using Robust.Shared.Toolshed;

namespace Content.Ratbite.Server.Bank;

[ToolshedCommand, AdminCommand(AdminFlags.Admin)]
public sealed class EvilBankCommand : ToolshedCommand
{
    private BankManager? _bank;

    [CommandImplementation("add")]
    public int Add([PipedArgument] ICommonSession user, [CommandArgument] int amount)
    {
        _bank ??= IoCManager.Resolve<BankManager>();

        if (amount <= 0)
            return 0;

        return _bank.ModifyShitcoins(user.UserId, amount);
    }

    [CommandImplementation("remove")]
    public int Remove([PipedArgument] ICommonSession user, [CommandArgument] int amount)
    {
        return Add(user, -amount);
    }

    [CommandImplementation("set")]
    public int Set([PipedArgument] ICommonSession user, [CommandArgument] int amount)
    {
        _bank ??= IoCManager.Resolve<BankManager>();

        var targetAmount = Math.Clamp(amount, 0, 100000);
        return _bank.SetShitcoins(user.UserId, targetAmount);
    }

    [CommandImplementation("get")]
    public int Get([PipedArgument] ICommonSession user)
    {
        _bank ??= IoCManager.Resolve<BankManager>();

        return _bank.GetShitcoins(user.UserId);
    }
}
