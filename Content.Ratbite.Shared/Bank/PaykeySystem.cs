using Content.Shared.Interaction.Events;

namespace Content.Ratbite.Shared.Bank;

public sealed partial class PaykeySystem : EntitySystem
{
    [Dependency] private SharedBankSystem _bank = default!;
    [Dependency] private SharedUserInterfaceSystem _ui = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<PaykeyComponent, UseInHandEvent>(OnInteract);
    }

    private void OnInteract(Entity<PaykeyComponent> ent, ref UseInHandEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        _ui.TryToggleUi(ent.Owner, PaykeyUiKey.Key, args.User);
        _ui.SetUiState(ent.Owner, PaykeyUiKey.Key, new PaykeyInterfaceState(_bank.GetAllBanks()));
    }
}
