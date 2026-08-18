using Content.Shared.Popups;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Whitelist;
using Robust.Shared.Timing;

namespace Content.Shared._BRatbite.LimitWeapon;

/// <summary>
/// This handles...
/// </summary>
public sealed class LimitWeaponSystem : EntitySystem
{
    [Dependency] private EntityWhitelistSystem _entityWhitelistSystem = default!;
    [Dependency] private SharedPopupSystem _popupSystem = default!;
    [Dependency] private IGameTiming _timing = default!;
    /// <inheritdoc/>
    public override void Initialize()
    {
        SubscribeLocalEvent<LimitWeaponComponent, ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<LimitWeaponComponent, AttemptMeleeEvent>(OnAttemptAttack);
    }

    private void OnAttemptAttack(Entity<LimitWeaponComponent> ent, ref AttemptMeleeEvent args)
    {
        if (args.Weapon == ent.Owner)
            return;

        if (ent.Comp.Whitelist == null)
        {
            args.Cancelled = true;
            ShowPopup(ent.Comp.MeleeFail, ent);
            return;
        }

        if (!_entityWhitelistSystem.IsValid(ent.Comp.Whitelist, args.Weapon))
        {
            args.Cancelled = true;
            ShowPopup(ent.Comp.MeleeFail, ent);
            return;
        }
    }

    private void OnAttemptShoot(Entity<LimitWeaponComponent> ent, ref ShotAttemptedEvent args)
    {
        if (args.Used.Owner == ent.Owner)
            return;

        if (ent.Comp.Whitelist == null)
        {
            args.Cancel();
            ShowPopup(ent.Comp.GunFail, ent);
            return;
        }

        if (!_entityWhitelistSystem.IsValid(ent.Comp.Whitelist, args.Used))
        {
            args.Cancel();
            ShowPopup(ent.Comp.GunFail, ent);
            return;
        }
    }

    private void ShowPopup(string text, Entity<LimitWeaponComponent> ent)
    {
        var time = _timing.CurTime;

        if (time > ent.Comp.LastPopup + ent.Comp.PopupCooldown)
        {
            ent.Comp.LastPopup = time;
            _popupSystem.PopupClient(Loc.GetString(text), ent, ent);
        }
    }
}
