using Content.Ratbite.Shared.Bank;

namespace Content.Ratbite.Server.Bank;

public sealed partial class BankSystem : SharedBankSystem
{
    private Dictionary<int, int> _accounts = new();

}
