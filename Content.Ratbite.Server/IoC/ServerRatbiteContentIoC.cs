using Content.Ratbite.Server.Bank;
using Content.Ratbite.Server.PermaBrig;

namespace Content.Ratbite.Server.IoC;

internal static class ServerRatbiteContentIoC
{
    internal static void Register(IDependencyCollection instance)
    {
        instance.Register<PermaBrigManager>();
        instance.Register<BankManager>();
    }
}
