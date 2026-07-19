using Content.Shared.Cuffs;
using Content.Shared.Cuffs.Components;
using Content.Shared.Interaction;
using Content.Shared.Mind;
using Content.Shared.Popups;
using Content.Shared.Mind.Components;
using Content.Goobstation.Shared.Fax;
using Content.Shared.Paper;
using System.Linq;
using Content.Server.Fax;
using Content.Shared.Fax.Components;
using Content.Server.Administration.Notes;
using Robust.Shared.Player;
using Robust.Shared.Network;
using Content.Shared.Database;
using System.Threading.Tasks;

namespace Content.Server._BRatbite.PermaBrig.NTRTermination;

public sealed partial class NTRTerminationSystem : EntitySystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mindSystem = default!;
    [Dependency] private readonly SharedCuffableSystem _cuffableSystem = default!;
    [Dependency] private readonly PermaBrigManager _permaBrigManager = default!;
    [Dependency] private readonly FaxSystem _faxSystem = default!;
    [Dependency] private readonly IAdminNotesManager _adminNotesManager = default!;
    [Dependency] private readonly ISharedPlayerManager _playerMan = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<NTRTerminatableComponent, MindAddedMessage>(OnMindAdded);
        SubscribeLocalEvent<NTRTerminatableComponent, MapInitEvent>(OnMapInit);

        SubscribeLocalEvent<NTRTerminationPaperComponent, MapInitEvent>(OnPaperInit);
        SubscribeLocalEvent<NTRTerminationPaperComponent, AfterInteractEvent>(OnInteract);
        SubscribeLocalEvent<NTRTerminationPaperComponent, GettingFaxedSentEvent>(OnGettingFaxed);
        SubscribeLocalEvent<NTRTerminationPaperComponent, InteractUsingEvent>(OnInteractUsing);
        base.Initialize();
    }

    private void OnInteract(Entity<NTRTerminationPaperComponent> ent, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Handled || args.Target is not { } target) return;
        if (!HasComp<NTRTerminationComponent>(args.User))
        {
            _popup.PopupEntity(Loc.GetString("ntr-termination-only-ntr"), args.User, args.User);
            return;
        }

        if (TryComp<CuffableComponent>(target, out var cuffableComp) && !_cuffableSystem.IsCuffed((target, cuffableComp)))
        {
            _popup.PopupEntity(Loc.GetString("ntr-termination-need-cuff"), args.User, args.User);
            return;
        }

        if (!TryComp<NTRTerminatableComponent>(args.Target, out var ntrTerminatable) || ntrTerminatable.LastMind is not { } userId)
        {
            _popup.PopupEntity(Loc.GetString("ntr-termination-only-command"), args.User, args.User);
            return;
        }

        if (!_mindSystem.TryGetMind(args.User, out _, out var mind) || mind.UserId is not { } terminator) return;

        ent.Comp.Terminator = terminator;
        ent.Comp.TerminatedUser = userId;
        _popup.PopupEntity(Loc.GetString("ntr-termination-victim-success"), args.User);
        args.Handled = true;
    }

    private void OnMindAdded(Entity<NTRTerminatableComponent> ent, ref MindAddedMessage args)
    {
        ent.Comp.LastMind = args.Mind.Comp.UserId;
    }

    private void OnMapInit(Entity<NTRTerminatableComponent> ent, ref MapInitEvent args)
    {
        _mindSystem.TryGetMind(ent, out _, out var mindComp);
        ent.Comp.LastMind = mindComp?.UserId;
    }

    private void OnGettingFaxed(Entity<NTRTerminationPaperComponent> ent, ref GettingFaxedSentEvent args)
    {
        if (ent.Comp.TerminatedUser is not { } user) return;
        if (!TryComp<PaperComponent>(ent, out var paper)) return;
        if (ent.Comp.Terminator is not { } terminator) return;
        if (ent.Comp.ForgedBy is { } forgedBy)
        {
            args.Handled = true;
            ReceiveFax(ent, args.Fax, Loc.GetString(ent.Comp.ForgedMessage));
            _permaBrigManager.AddBrigTime(forgedBy, (int) ent.Comp.AddedTime.TotalMinutes * 3);
            // Terminator and forgedBy will pretty much always be the same person
            AddAdminRemark(terminator, forgedBy, Loc.GetString("ntr-termination-forge-admin-remark"));
            RemComp<NTRTerminationPaperComponent>(ent);
            return;
        }
        var isStamped = ent.Comp.AcceptedStamps.Any(
            locId => paper.StampedBy.Select(p => p.StampedName).Contains(locId)
        );
        if (!isStamped) return;

        var reasonForDemotion = FindReasonOfDemotion(paper.Content);

        ReceiveFax(ent, args.Fax, Loc.GetString(ent.Comp.AcceptedMessage, [("reason", reasonForDemotion)]));
        _permaBrigManager.AddBrigTime(user, (int) ent.Comp.AddedTime.TotalMinutes);
        AddAdminRemark(terminator, user, Loc.GetString("ntr-termination-success-admin-remark", [("reason", reasonForDemotion)]));
        // Remove component so they can't brig the same person again
        RemComp<NTRTerminationPaperComponent>(ent);
        args.Handled = true;
    }

    private string FindReasonOfDemotion(string content)
    {
        var reasonField = Loc.GetString("ntr-termination-paper-reson-for-demotion");
        var startIndex = content.IndexOf(reasonField);
        if (startIndex == -1) // NTR messed up the form, I guess take the last line
        {
            var line = content.Split("\n").LastOrDefault((l) => l.Trim().Length > 0) ?? "";
            return line.Substring(0, Math.Min(300, line.Length));
        }
        var field = content.Substring(startIndex + reasonField.Length);
        var result = (string.Join("\n", field.Split("\n").Take(3))).Trim();
        return result.Substring(0, Math.Min(300, result.Length));
    }

    private void ReceiveFax(Entity<NTRTerminationPaperComponent> ent, EntityUid fax, string message)
    {
        _faxSystem.Receive(
            fax,
            new FaxPrintout(
                message,
                new(),
                Loc.GetString("ntr-termination-paper-name"),
                stampState: "paper_stamp-centcom",
                stampedBy: new List<StampDisplayInfo> {
                    new StampDisplayInfo {
                        StampedName = ent.Comp.AcceptedStampName,
                        StampedColor = ent.Comp.AcceptedStampColor
                    }
                }
            )
        );
    }

    private void AddAdminRemark(NetUserId sender, NetUserId target, string message)
    {
        var session = _playerMan.GetSessionById(sender);
        Task.Run(() => _adminNotesManager.AddAdminRemark(session, target, NoteType.Note, message, NoteSeverity.Minor, false, null)).GetAwaiter().GetResult();
    }

    private void OnInteractUsing(Entity<NTRTerminationPaperComponent> ent, ref InteractUsingEvent args)
    {
        if (!TryComp<StampComponent>(args.Used, out var stampComp) || !HasComp<NTRTerminationComponent>(args.User)) return;
        if (ent.Comp.AcceptedStamps.Contains(stampComp.StampedName) && _mindSystem.TryGetMind(args.User, out _, out var mind))
        {
            // NTR is a moron and is trying to forge the stamp
            ent.Comp.ForgedBy = mind.UserId;
        }
    }

    private void OnPaperInit(Entity<NTRTerminationPaperComponent> ent, ref MapInitEvent args)
    {
        if (!TryComp<PaperComponent>(ent, out var paper)) return;
        paper.Content += '\n' + Loc.GetString("ntr-termination-paper-reson-for-demotion");
    }
}
