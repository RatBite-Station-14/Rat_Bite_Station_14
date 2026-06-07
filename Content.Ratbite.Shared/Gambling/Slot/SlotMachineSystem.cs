using System.Linq;
using Content.Shared.Chat;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Emag.Systems;
using Content.Shared.Interaction;
using Content.Shared.Popups;
using Content.Shared.Power.EntitySystems;
using Content.Shared.Random.Helpers;
using Content.Shared.Stacks;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Ratbite.Shared.Gambling.Slot;

public sealed partial class SlotMachineSystem : EntitySystem
{
    [Dependency] private IGameTiming _timing = default!;
    [Dependency] private INetManager _net = default!;
    [Dependency] private IPrototypeManager _proto = default!;
    [Dependency] private SharedAudioSystem _audio = default!;
    [Dependency] private SharedDoAfterSystem _doAfter = default!;
    [Dependency] private ItemSlotsSystem _itemSlots = default!;
    [Dependency] private SharedPopupSystem _popup = default!;
    [Dependency] private SharedChatSystem _chatSystem = default!;
    [Dependency] private SharedPowerReceiverSystem _power = default!;
    [Dependency] private SharedStackSystem _stack = default!;
    [Dependency] private SharedAppearanceSystem _appearance = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SlotMachineComponent, ActivateInWorldEvent>(OnInteractHandEvent);
        SubscribeLocalEvent<SlotMachineComponent, SlotMachineDoAfterEvent>(OnSlotMachineDoAfter);
        SubscribeLocalEvent<SlotMachineComponent, GotEmaggedEvent>(OnEmagged);
    }

    /// <summary>
    /// For whenever it gets emagged
    /// </summary>
    private void OnEmagged(EntityUid uid, SlotMachineComponent comp, ref GotEmaggedEvent args)
    {
        if (comp.Emagged)
            return;

        args.Handled = true;
        comp.Emagged = true;

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(uid));
        comp.SpinCost = random.Next(50, 100000);
        comp.SmallPrizeAmount = random.Next(-500, 5000);
        comp.MediumPrizeAmount = random.Next(-500, 10000);
        comp.BigPrizeAmount = random.Next(-500, 50000);
        comp.JackPotPrizeAmount = random.Next(-500, 100000);

        comp.SmallWinChance = random.NextFloat(0, 0.6f);
        comp.MediumWinChance = random.NextFloat(0, 0.35f);
        comp.BigWinChance = random.NextFloat(0f, 0.2f);
        comp.JackPotWinChance = random.NextFloat(0, 0.1f);
        comp.GodPotWinChance = random.NextFloat(0, 0.05f);

        // lord have mercy...
        var allProtos = _proto.EnumeratePrototypes<EntityPrototype>().ToList();

        if (allProtos.Count > 0)
        {
            var randomProto = random.Pick(allProtos);
            comp.GodPotPrize = randomProto.ID;
        }
    }

    /// <summary>
    /// Handle the logic for starting the slot machine
    /// </summary>

    private void OnInteractHandEvent(EntityUid uid, SlotMachineComponent comp, ActivateInWorldEvent args)
    {
        if (comp.IsSpinning || !_power.IsPowered(uid))
            return;

        if (!_itemSlots.TryGetSlot(uid, "money", out var slot)
            || slot.Item == null
            || !TryComp<StackComponent>(slot.Item.Value, out var stack)
            || stack.Count < comp.SpinCost)
        {
            _popup.PopupPredicted(Loc.GetString("slotmachine-no-money"), uid, uid, PopupType.Small); // No Money
            return;
        }

        var doAfter =
         new DoAfterArgs(EntityManager, uid, comp.DoAfterTime, new SlotMachineDoAfterEvent(), uid)
         {
             BreakOnMove = false,
             BreakOnDamage = false,
             MultiplyDelay = false,
         };

        _stack.SetCount(stack.Owner, stack.Count - comp.SpinCost, stack);
        Dirty(stack.Owner, stack);
        comp.IsSpinning = true;

        if (_net.IsServer)
        {
            _audio.PlayPvs(comp.SpinSound, uid);
            _doAfter.TryStartDoAfter(doAfter);
        }
        if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
        {
            _appearance.SetData(uid, SlotMachineVisuals.Spinning, true);
        }
    }

    private void OnSlotMachineDoAfter(EntityUid uid, SlotMachineComponent comp, SlotMachineDoAfterEvent args)
    {
        if (args.Cancelled) // Almost no way for it to be canceled but just in case
        {
            comp.IsSpinning = false;
            Dirty(uid, comp);
            return;
        }

        if (args.Handled || !_itemSlots.TryGetSlot(uid, "money", out var slot))
            return;

        comp.IsSpinning = false;
        Dirty(uid, comp);

        if (TryComp<AppearanceComponent>(uid, out var appearance) && _net.IsServer)
        {
            _appearance.SetData(uid, SlotMachineVisuals.Spinning, false);
        }

        // Handle the chances
        StackComponent? stack = null;
        if (slot.Item != null)
            TryComp(slot.Item.Value, out stack);

        var random = SharedRandomExtensions.PredictedRandom(_timing, GetNetEntity(uid));

        if (random.Prob(comp.SmallWinChance))
        {
            _audio.PlayPredicted(comp.SmallWinSound, uid, args.User);
            HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.SmallPrizeAmount)), stack, comp.SmallPrizeAmount);
            return;
        }
        if (random.Prob(comp.MediumWinChance))
        {
            _audio.PlayPredicted(comp.MediumWinSound, uid, args.User);
            HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.MediumPrizeAmount)), stack, comp.MediumPrizeAmount);
            return;
        }
        if (random.Prob(comp.BigWinChance))
        {
            _audio.PlayPredicted(comp.BigWinSound, uid, args.User);
            HandlePrize(uid, Loc.GetString("slotmachine-win-normal", ("amount", comp.BigPrizeAmount)), stack, comp.BigPrizeAmount);
            return;
        }
        if (random.Prob(comp.JackPotWinChance))
        {
            _audio.PlayPredicted(comp.JackPotWinSound, uid, args.User);
            HandlePrize(uid, Loc.GetString("slotmachine-win-jackpot"), stack, comp.JackPotPrizeAmount);
            return;
        }
        if (random.Prob(comp.GodPotWinChance)) // THE GODPOT!!!
        {
            _audio.PlayPredicted(comp.GodPotWinSound, uid, args.User);
            var coordinates = Transform(uid).Coordinates;
            PredictedSpawnAtPosition(comp.GodPotPrize, coordinates);
            _chatSystem.TrySendInGameICMessage(uid, Loc.GetString("slotmachine-win-godpot"), InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
            return;
        }

        _audio.PlayPredicted(comp.LoseSound, uid, args.User); // If nothing then lose
    }

    private void HandlePrize(EntityUid uid, string msg, StackComponent? stack, int prize)
    {
        if (stack is { })
        {
            // Add money to the stack and play a message
            _stack.SetCount(stack.Owner, stack.Count + prize, stack);
            Dirty(stack.Owner, stack);
            _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
            return;
        }

        // Spawn a new cash stack if there's no money left in the machine
        var coordinates = Transform(uid).Coordinates;
        var newStack = PredictedSpawnAtPosition("SpaceCash", coordinates);
        if (TryComp<StackComponent>(newStack, out var newStackComp))
        {
            _stack.SetCount(newStack, prize, newStackComp);
            Dirty(newStack, newStackComp);
        }

        _chatSystem.TrySendInGameICMessage(uid, msg, InGameICChatType.Speak, hideChat: false, hideLog: true, checkRadioPrefix: false);
        return;
    }
}
