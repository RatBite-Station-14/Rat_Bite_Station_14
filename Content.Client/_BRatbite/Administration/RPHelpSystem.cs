using Content.Client.UserInterface.Systems.Bwoink;
using Content.Shared._BRatbite.Administration;
using Content.Shared.Implants.Components;
using Robust.Client.Player;
using Robust.Client.UserInterface;
using Robust.Shared.Timing;
using static Content.Shared.Administration.SharedBwoinkSystem;

namespace Content.Client._BRatbite.Administration;

public sealed partial class RPHelpSystem : SharedRPHelpSystem
{
    [Dependency] private readonly IUserInterfaceManager _ui = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IPlayerManager _playerManager = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<SubdermalImplantComponent, RPHelpActionEvent>(OnRPHelpAction);
    }

    private void OnRPHelpAction(Entity<SubdermalImplantComponent> ent, ref RPHelpActionEvent args)
    {
        if (!_timing.IsFirstTimePredicted) return;
        if (_playerManager.LocalEntity != args.Performer) return;
        if (!HasComp<RPHelpComponent>(args.Performer)) return;
        var ahelpUIController = _ui.GetUIController<AHelpUIController>();
        ahelpUIController.ToggleWindow(BwoinkType.RPHelp);
    }
}
