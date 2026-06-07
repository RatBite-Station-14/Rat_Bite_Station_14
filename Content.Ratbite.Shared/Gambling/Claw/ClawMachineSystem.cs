using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Ratbite.Shared.Gambling.Claw;

/// <summary>
/// This handles the coinflipper machine logic
/// </summary>
public sealed partial class ClawMachineSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedPowerReceiverSystem _power = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<ClawMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<ClawMachineComponent, ClawGameDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<ClawMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    private void OnEmagged(EntityUid uid, ClawMachineComponent comp, ref GotEmaggedEvent args)
    {
        if (comp.Emagged)
            return;

        args.Handled = true;
        comp.Emagged = true;

        comp.Rewards = comp.EvilRewards; //My name is nhoj nhoj and I am EVIL
    }
    private void OnInteractHandEvent(EntityUid uid, ClawMachineComponent comp, ActivateInWorldEvent args)
    {
        if (comp.IsSpinning || !_power.IsPowered(uid))
            return;

        var doAfter =
         new DoAfterArgs(EntityManager, args.User, comp.DoAfterTime, new ClawGameDoAfterEvent(), uid)
         {
             BreakOnMove = true,
             BreakOnDamage = true,
             MultiplyDelay = false,
         };
        comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(comp.PlaySound, uid);
            _doAfter.TryStartDoAfter(doAfter);
        }
        if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
        {
            _appearance.SetData(uid, ClawMachineVisuals.Spinning, true);
            _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, false);
        }
    }

    private void OnSlotMachineDoAfter(EntityUid uid, ClawMachineComponent comp, ClawGameDoAfterEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        if (args.Cancelled)
        {
            var selfMsgFail = Loc.GetString("clawmachine-fail-self");
            var othersMsgFail = Loc.GetString("clawmachine-fail-other", ("user", args.User));
            comp.IsSpinning = false;
            _popup.PopupPredicted(selfMsgFail, othersMsgFail, args.User, args.User, PopupType.Small);
            if (TryComp<AppearanceComponent>(uid, out var _) && _net.IsServer)
            {
                _appearance.SetData(uid, ClawMachineVisuals.Spinning, false);
                _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, true);
            }
            Dirty(uid, comp);
            return;
        }

        if (TryComp<AppearanceComponent>(uid, out var _) && _net.IsServer)
        {
            _appearance.SetData(uid, ClawMachineVisuals.Spinning, false);
            _appearance.SetData(uid, ClawMachineVisuals.NormalSprite, true);
        }
        comp.IsSpinning = false;
        Dirty(uid, comp);
        if (!_net.IsServer)
            return;

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(uid));
        if (!(random.Prob(comp.WinChance) && comp.Rewards != null))
        {
            _popup.PopupEntity(Loc.GetString("clawmachine-fail-generic"), uid);
            _audio.PlayPvs(comp.LoseSound, uid);
            return;
        }

        _audio.PlayPvs(comp.WinSound, uid);
        var rewardToSpawn = random.Pick(comp.Rewards);
        var coordinates = Transform(uid).Coordinates;
        PredictedSpawnAtPosition(rewardToSpawn, coordinates);
    }
}
