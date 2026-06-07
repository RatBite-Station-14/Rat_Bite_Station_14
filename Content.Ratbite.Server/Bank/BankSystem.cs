using Content.Ratbite.Shared.Bank;
using Content.Server.Mind;
using Content.Server.Stack;
using Content.Server.StationRecords.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Components;
using Content.Shared.GameTicking;
using Content.Shared.Humanoid;
using Content.Shared.Popups;
using Content.Shared.Stacks;
using Content.Shared.StationRecords;
using Content.Shared.Store;
using Content.Shared.Store.Components;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Ratbite.Server.Bank;

public sealed partial class BankSystem : SharedBankSystem
{
    //[Dependency] private IPlayerManager _player = default!;
    [Dependency] private MindSystem _mind = default!;
    [Dependency] private BankManager _bank = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private IPrototypeManager _proto = default!;

    [Dependency] private EntityQuery<FingerprintComponent> _fingerQuery = default!;
    [Dependency] private EntityQuery<DnaComponent> _dnaQuery = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<HumanoidProfileComponent, PlayerSpawnCompleteEvent>(OnSpawn);
        SubscribeLocalEvent<PaykeyComponent, BankSendToOOCMessage>(OnSendMoneyOOC);
    }

    private void OnSpawn(Entity<HumanoidProfileComponent> ent, ref PlayerSpawnCompleteEvent args)
    {
        if (!_mind.TryGetMind(ent.Owner, out var mind, out var mindComp))
            return;

        GetNetEntity(mind);
    }

    private void OnSendMoneyOOC(Entity<PaykeyComponent> ent, ref BankSendToOOCMessage args)
    {
        if (FindPerson(ent.Owner) is not { } player)
        {
            _popup.PopupEntity("Transaction Failed: Bank Terminal Malfunction Detected - Could not determine owner.", ent.Owner);
            return;
        }

        var bank = GetEntity(args.Bank);

        if (!TryComp<BankComponent>(bank, out var bankComp))
        {
            _popup.PopupEntity("Transaction Failed: Bank Terminal Malfunction Detected - Bank not found.", ent.Owner);
            return;
        }

        var bankEnt = (bank, bankComp);

        if (!bankComp.Accounts.ContainsKey(args.Account))
        {
            _popup.PopupEntity($"Transaction Failed: Bank Terminal Malfunction Detected - {args.Account} does not exist.", ent.Owner);
            return;
        }

        if (!IsPasswordValid(args.Account, args.Password, bankEnt))
        {
            _popup.PopupEntity("Transaction Failed: Bank Terminal Malfunction Detected - Invalid password.", ent.Owner);
            return;
        }

        var money = _proto.Index(bankComp.Currency);

        var transferedAmount = TransferCreditAccountsOOC(bankEnt, args.Account, player.Comp.PlayerSession.UserId, args.Amount, money.ConversionRateOOC);

        _audio.PlayPvs(ent.Comp.SoundOnTransfer, ent);
        if (transferedAmount == 0)
        {
            _popup.PopupEntity("Transaction Failed: Insufficient Funds Detected.", ent.Owner);
            return;
        }
        _popup.PopupEntity($"{Name(args.Actor)} paid {Name(player.Owner)} {args.Amount} credits to an offshore bank account.", ent.Owner);
    }

    public int TransferCreditAccountsOOC(Entity<BankComponent> ent, string moneyAccount, NetUserId transferAccount, FixedPoint2 amount, FixedPoint2 conversionRate)
    {
        var money = ent.Comp.Accounts.GetValueOrDefault(moneyAccount, 0);
        var moneyToTransfer = (FixedPoint2.Max(money - amount, 0) * conversionRate).Int();
        ent.Comp.Accounts[moneyAccount] = money - (moneyToTransfer / conversionRate);
        _bank.ModifyShitcoins(transferAccount, moneyToTransfer);
        Dirty(ent);
        return moneyToTransfer;
    }

    public Entity<ActorComponent>? FindPerson(EntityUid keyCard)
    {
        if (!TryComp<StationRecordKeyStorageComponent>(keyCard, out var keyStorage))
            return null;

        if (keyStorage?.Key is not { } recordKey)
            return null;

        if (!_records.TryGetRecord<GeneralStationRecord>(recordKey, out var record))
            return null;

        // TODO: Fix this shit, ~100 list check is total ass.
        var allHumanoids = EntityQueryEnumerator<ActorComponent>();
        while (allHumanoids.MoveNext(out var uid, out var meta))
        {
            if (!_dnaQuery.TryComp(uid, out var dnaComp))
                continue;

            if (record.DNA != dnaComp.DNA)
                continue;

            return (uid, meta);
        }

        return null;
    }
}
