using Content.Shared._BRatbite.Atmos;
using Content.Shared.UserInterface;
using Robust.Server.GameObjects;

namespace Content.Server._BRatbite.Atmos;

public sealed class GasMinerComputerSystem : EntitySystem
{
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<GasMinerComputerComponent, BeforeActivatableUIOpenEvent>(OnBeforeOpened);

    }

    private void OnBeforeOpened(Entity<GasMinerComputerComponent> ent, ref BeforeActivatableUIOpenEvent args)
    {
        DirtyUI(ent);
    }

    private void DirtyUI(Entity<GasMinerComputerComponent> ent)
    {
        _userInterfaceSystem.SetUiState(ent.Owner, GasMinerComputerUiKey.Key, new GasMinerComputerBoundUserInterfaceState());
    }
}
