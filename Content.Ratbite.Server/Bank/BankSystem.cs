using Content.Ratbite.Shared.Bank;
using Content.Server.Preferences.Managers;
using Content.Server.StationRecords.Systems;
using Content.Shared.FixedPoint;
using Content.Shared.Forensics.Components;
using Content.Shared.GameTicking;
using Content.Shared.Popups;
using Content.Shared.StationRecords;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Player;

namespace Content.Ratbite.Server.Bank;

public sealed partial class BankSystem : SharedBankSystem
{
    //[Dependency] private IPlayerManager _player = default!;
    //[Dependency] private MindSystem _mind = default!;
    [Dependency] private BankManager _bank = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private StationRecordsSystem _records = default!;
    [Dependency] private IServerPreferencesManager _pref = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private IServerNetManager _net = default!;

    [Dependency] private EntityQuery<FingerprintComponent> _fingerQuery = default!;
    [Dependency] private EntityQuery<DnaComponent> _dnaQuery = default!;

    private Dictionary<NetUserId, int> _moneyToBring = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaykeyComponent, BankSendToOOCMessage>(OnSendMoneyOOC);

        _net.RegisterNetMessage<MsgRequestBankBalance>(OnRequestBalance);
        _net.RegisterNetMessage<MsgUpdateLobbyBringAmount>(OnUpdateBringAmount);
        _net.RegisterNetMessage<MsgBankBalanceResponse>();
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

    private void OnRequestBalance(MsgRequestBankBalance message)
    {
        var senderSession = message.MsgChannel.UserId;
        var response = new MsgBankBalanceResponse { Balance = _bank.GetShitcoins(senderSession) };
        _net.ServerSendMessage(response, message.MsgChannel);
    }

    private void OnUpdateBringAmount(MsgUpdateLobbyBringAmount message)
    {
        var senderSession = message.MsgChannel.UserId;
        int clampedAmount = Math.Clamp(message.Balance, 0, _bank.GetShitcoins(senderSession));
        var prefs = _pref.GetPreferences(senderSession);
        foreach (var (id, character) in prefs.Characters)
        {
            character.Credits = clampedAmount;
            _pref.SetProfile(senderSession, id, character);
        }
        _moneyToBring[senderSession] = clampedAmount;
        Log.Info($"Player {senderSession} locked in {clampedAmount} credits for round deployment.");
    }

    public override void OnPlayerSpawnComplete(PlayerSpawnCompleteEvent ev)
    {
        base.OnPlayerSpawnComplete(ev);

        var userId = ev.Player.UserId;

        if (!_moneyToBring.Remove(userId, out var spentAmount) || spentAmount < 0)
            return;

        _bank.ModifyShitcoins(userId, -spentAmount);
        Logger.GetSawmill("server_bank").Info($"Deducted {spentAmount} credits from persistent profile account of {ev.Player.Name} upon spawn completion.");
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
