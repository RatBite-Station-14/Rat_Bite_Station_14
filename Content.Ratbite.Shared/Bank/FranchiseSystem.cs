using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.UserInterface;

namespace Content.Ratbite.Shared.Bank;

public sealed partial class FranchiseSystem : EntitySystem
{
    [Dependency] private SharedBankSystem _bank = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<FranchiseTerminalComponent, BeforeActivatableUIOpenEvent>(OnOpen);
        SubscribeLocalEvent<FranchiseTerminalComponent, InteractUsingEvent>(OnInsertPaykey);
    }

    private void OnOpen(Entity<FranchiseTerminalComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        if (!TryComp<BankTerminalComponent>(ent.Owner, out var bankTerm))
            return;

        _ui.SetUiState(ent.Owner, FranchiseTerminalUiKey.Key, new PaykeyInterfaceState(_bank.GetAllBanks()));
        _ui.SetUiState(ent.Owner, FranchiseTerminalUiKey.Key, new BankTerminalInterfaceState(bankTerm.LinkedAccount, bankTerm.LinkedPassword, GetNetEntity(bankTerm.LinkedBank)));
    }

    private void OnInsertPaykey(Entity<FranchiseTerminalComponent> ent, ref InteractUsingEvent args)
    {
        if (args.Handled || !TryComp<PaykeyComponent>(args.Used, out var paykey))
            return;

        if (paykey.Franchise is { })
        {
            _popup.PopupPredicted($"Already paired to a franchise.", ent, ent, PopupType.Medium);
            return;
        }

        paykey.Franchise = ent.Comp.Franchise;
        Dirty(args.Used, paykey);
    }
}
