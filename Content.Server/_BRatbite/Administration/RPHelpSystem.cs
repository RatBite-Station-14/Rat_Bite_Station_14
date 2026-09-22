using Content.Server.Administration.Managers;
using Content.Server.Administration.Systems;
using Content.Shared._BRatbite.Administration;
using Content.Shared.Mobs.Systems;
using static Content.Shared.Administration.SharedBwoinkSystem;

namespace Content.Server._BRatbite.Administration;

public sealed partial class RPHelpSystem : SharedRPHelpSystem
{
    [Dependency] private readonly BwoinkSystem _bwoinkSystem = default!;
    [Dependency] private readonly MobStateSystem _mobStateSystem = default!;
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BeforeBwoinkMessageSentEvent>(OnBeforeBwoinkMessage);
    }

    private void OnBeforeBwoinkMessage(ref BeforeBwoinkMessageSentEvent args)
    {
        if (args.IsAdmin || args.Message.Type != BwoinkType.RPHelp || args.SenderSession.AttachedEntity is not { } ent) return;
        args.Cancelled = !HasComp<RPHelpComponent>(ent) || !_mobStateSystem.IsAlive(ent);
        if (args.Cancelled)
            RaiseNetworkEvent(new BwoinkTextMessage(args.SenderSession.UserId, default, Loc.GetString("rp-help-not-allowed"), type: BwoinkType.RPHelp), args.SenderSession.Channel);
    }
}
