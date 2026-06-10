using System.Linq;
using Content.Shared.Chat;
using Content.Shared.Coordinates;
using Content.Shared.FixedPoint;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Timing;
using Robust.Shared.Utility;

namespace Content.Ratbite.Shared.Bank;

public abstract partial class SharedBankSystem : EntitySystem
{
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedStackSystem _stack = default!;
    [Dependency] private SharedChatSystem _chat = default!;
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private INetManager _net = default!;

    private List<NetEntity> _banks = new();
    private List<(NetEntity, int)> _accountQueue = new();
    private TimeSpan _accountCheck = TimeSpan.Zero;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PlayerSpawnCompleteEvent>(OnPlayerSpawnComplete);
        SubscribeLocalEvent<BankComponent, ComponentRemove>(OnBankDestroyed);
    }

    public virtual void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent args)
    {
        var moneyMarket = (GetNetEntity(args.Mob), args.Profile.Credits);
        if (GetAllBanks().Count == 0)
        {
            _accountQueue.Add(moneyMarket);
            return;
        }

        var bank = GetEntity(GetAllBanks().First());
        if (!TryComp<BankComponent>(bank, out var bankComp))
            return;

        AddAccount((bank, bankComp), args.Mob, args.Profile.Credits);
    }

    private void OnBankDestroyed(Entity<BankComponent> ent, ref ComponentRemove args)
    {
        var currency = _proto.Index(ent.Comp.Currency);
        foreach (var (_, money) in ent.Comp.Accounts)
        {
            PrintMoney(ent, money, currency);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (_timing.CurTime < _accountCheck)
            return;

        _accountCheck = _timing.CurTime + TimeSpan.FromSeconds(1);

        if (GetAllBanks().FirstOrNull() is not { } bank || !TryComp<BankComponent>(GetEntity(bank), out var bankComp))
            return;

        var bankent = (GetEntity(bank), bankComp);

        foreach (var (netEntity, money) in _accountQueue)
        {
            AddAccount(bankent, GetEntity(netEntity), money);
        }
    }

    public List<NetEntity> GetAllBanks()
    {
        _banks.Clear();
        if (_net.IsClient)
            return _banks;

        var query = EntityQueryEnumerator<BankComponent>();
        while (query.MoveNext(out var id, out _))
        {
            _banks.Add(GetNetEntity(id));
        }
        return _banks;
    }

    public FixedPoint2 AddCreditsAccountIIC(Entity<BankComponent> ent, string account, FixedPoint2 amount)
    {
        var currentMoney = ent.Comp.Accounts.GetValueOrDefault(account, 0);
        ent.Comp.Accounts[account] = currentMoney + amount;
        Dirty(ent);
        return currentMoney + amount;
    }

    public void InsertMoney(Entity<BankTerminalComponent> terminal, EntityUid user, Entity<CurrencyComponent, StackComponent> creditItem)
    {
        if (!Exists(creditItem))
            return;

        if (terminal.Comp.LinkedBank is not { } bank || !TryComp<BankComponent>(bank, out var bankComp) || !creditItem.Comp1.Price.ContainsKey(bankComp.Currency))
            return;

        _stack.ReduceCount((creditItem, creditItem.Comp2), AddCreditsAccountIIC((bank, bankComp), terminal.Comp.LinkedAccount, creditItem.Comp2.Count * creditItem.Comp1.Price[bankComp.Currency]).Int());
    }

    /// <summary>
    /// Print money from something in to thin air on an entity.
    /// </summary>
    public FixedPoint2 PrintMoney(EntityUid uid, FixedPoint2 amount, CurrencyPrototype currency, bool popup = false)
    {
        if (amount <= 0 || !Exists(uid))
            return amount;

        if (currency.Cash is not { } cash)
            return amount;

        var amountRemaining = amount;
        var coordinates = uid.ToCoordinates();
        var sortedCashValues = cash.Keys.OrderByDescending(x => x).ToList();
        EntityUid? money = null;
        foreach (var value in sortedCashValues)
        {
            var cashId = cash[value];
            var amountToSpawn = (int) MathF.Floor((float) (amountRemaining / value));
            for (var i = 0; i < amountToSpawn; i++)
            {
                var spawned = PredictedSpawnAtPosition(cashId, coordinates);
                if (money is not { } existingMoney)
                    money = spawned;
                else
                    _stack.TryMergeStacks(spawned, existingMoney, out _);
            }
            amountRemaining -= value * amountToSpawn;
        }
        if (money is { } ent && popup)
            _popup.PopupPredictedCoordinates($"Printed {amount} {Name(ent)}.", coordinates, uid, PopupType.Medium);
        return amountRemaining;
    }


    public FixedPoint2 TransferCreditAccountsIIC(Entity<BankComponent> ent, string moneyAccount, string transferAccount, FixedPoint2 amount)
    {
        var money = ent.Comp.Accounts.GetValueOrDefault(moneyAccount, 0);
        var moneyToTransfer = FixedPoint2.Max(money - amount, 0);
        ent.Comp.Accounts[moneyAccount] = money - moneyToTransfer;
        ent.Comp.Accounts[transferAccount] = ent.Comp.Accounts.GetValueOrDefault(transferAccount, 0) + moneyToTransfer;
        Dirty(ent);
        return moneyToTransfer;
    }

    /// <summary>
    /// Can account be accessed from this bank?
    /// </summary>
    public Entity<BankComponent>? CanAccessAccount(EntityUid bank, string account, string password)
    {
        if (!TryComp<BankComponent>(bank, out var bankComp))
        {
            _popup.PopupEntity("Access Failed: Bank is invalid.", bank);
            return null;
        }

        if (!bankComp.Accounts.ContainsKey(account))
        {
            _popup.PopupEntity($"Access Failed: {account} does not exist.", bank);
            return null;
        }

        if (!IsPasswordValid(account, password, (bank, bankComp)))
        {
            _popup.PopupEntity("Access Failed: Incorrect Password", bank);
            return null;
        }

        return (bank, bankComp);
    }

    public (string account, string password) GenerateSevenId(NetEntity seed)
    {
        var random = SharedRandomExtensions.PredictedRandom(_timing, seed);
        var account = "";
        var password = "";
        for (var i = 0; i < 7; i++)
        {
            account += random.Next(10).ToString();
        }
        for (var i = 0; i < 7; i++)
        {
            password += random.Next(10).ToString();
        }
        return (account, password);
    }

    public void AddAccount(Entity<BankComponent> ent, EntityUid player, int money)
    {
        var (account, password) = GenerateSevenId(GetNetEntity(ent.Owner));

        ent.Comp.Accounts[account] = money;
        ent.Comp.Passwords[account] = password;
        Dirty(ent);

        // TODO: Add as memory in character menu.
    }

    public bool IsPasswordValid(string account, string password, Entity<BankComponent> bank)
        => bank.Comp.Passwords.TryGetValue(account, out var storedPassword) && storedPassword == password;
}
