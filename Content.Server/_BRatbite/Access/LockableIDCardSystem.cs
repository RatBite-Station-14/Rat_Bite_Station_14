using System.Text;
using Content.Shared._BRatbite.Access;
using Content.Shared.GameTicking;
using Content.Shared.Lock;
using Content.Shared.Paper;
using Content.Shared.Popups;
using Robust.Server.GameObjects;
using Robust.Shared.Random;

namespace Content.Server._BRatbite.Access;

public sealed partial class LockableIDCardSystem : SharedLockableIDCardSystem
{
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly LockSystem _lockSystem = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly PaperSystem _paper = default!;
    [Dependency] private readonly UserInterfaceSystem _userInterfaceSystem = default!;
    private static int _defaultCodeLength = 4;
    private string? _currentCode;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LockableIDCardComponent, LockableIDSendPasswordMessage>(OnLockableIDSendPassword);
        SubscribeLocalEvent<LockableIDCardComponent, MapInitEvent>(OnLockableIDCardInit);
        SubscribeLocalEvent<RoundRestartCleanupEvent>((_) => _currentCode = null);
        SubscribeLocalEvent<LockableIDCardPaperComponent, MapInitEvent>(OnPaperInit);
    }

    private void OnLockableIDSendPassword(Entity<LockableIDCardComponent> ent, ref LockableIDSendPasswordMessage args)
    {
        if (!TryComp<LockComponent>(ent, out var lockComp) || !lockComp.Locked) return;
        if (args.Password != ent.Comp.Password)
        {
            _popup.PopupCursor(Loc.GetString("lockable-id-card-wrong-password"), args.Actor);
            return;
        }
        _userInterfaceSystem.CloseUi(ent.Owner, LockableIDUiKey.Key);
        _lockSystem.Unlock(ent, null, lockComp);
    }

    private void OnLockableIDCardInit(Entity<LockableIDCardComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Password = GetOrGenerateRandomPassword();
    }

    private string GetOrGenerateRandomPassword()
    {
        if (_currentCode is not null) return _currentCode;
        var res = new StringBuilder(_defaultCodeLength);
        for (var i = 0; i < _defaultCodeLength; i++)
        {
            res.Append(_random.Next(0, 10).ToString());
        }
        _currentCode = res.ToString();
        return _currentCode;
    }

    private void OnPaperInit(Entity<LockableIDCardPaperComponent> ent, ref MapInitEvent args)
    {
        _paper.SetContent(ent, Loc.GetString(ent.Comp.PaperContent, [("code", GetOrGenerateRandomPassword())]));
        RemCompDeferred<LockableIDCardPaperComponent>(ent);
    }
}
