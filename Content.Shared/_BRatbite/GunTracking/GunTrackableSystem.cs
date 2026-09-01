using Content.Shared.Containers.ItemSlots;
using Content.Shared.DoAfter;
using Content.Shared.Electrocution;
using Content.Shared.Examine;
using Content.Shared.Interaction;
using Content.Shared.Mindshield.Components;
using Content.Shared.Popups;
using Content.Shared.Tools.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Whitelist;

namespace Content.Shared._BRatbite.GunTracking;

public sealed partial class GunTrackableSystem : EntitySystem
{
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedToolSystem _toolSystem = default!;
    [Dependency] private readonly SharedElectrocutionSystem _electrocutionSystem = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfterSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<GunTrackableComponent, ShotAttemptedEvent>(OnShotAttempted);
        SubscribeLocalEvent<GunTrackableComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<GunTrackableComponent, InteractUsingEvent>(OnInteractUsing);
        SubscribeLocalEvent<GunTrackableComponent, RemoveTrackerEvent>(OnRemoveTracker);
        SubscribeLocalEvent<GunTrackableComponent, MapInitEvent>(OnGunTrackableInit);
    }

    private void OnShotAttempted(Entity<GunTrackableComponent> ent, ref ShotAttemptedEvent args)
    {
        if (!IsTracked(ent)) return;
        if (HasComp<MindShieldComponent>(args.User) || HasComp<FakeMindShieldComponent>(args.User))
            return;
        _popup.PopupClient(Loc.GetString("firing-pin-cant-fire"), ent, args.User);
        args.Cancel();
    }

    private bool IsTracked(Entity<GunTrackableComponent> ent)
    {
        return _itemSlots.TryGetSlot(ent.Owner, ent.Comp.GunTrackerSlotId, out var slot) && slot.Item is not null;
    }

    private void OnExamined(Entity<GunTrackableComponent> ent, ref ExaminedEvent args)
    {
        if (!IsTracked(ent)) return;
        args.PushMarkup(Loc.GetString("gun-trackable-tracked"));
    }

    private void OnInteractUsing(Entity<GunTrackableComponent> ent, ref InteractUsingEvent args)
    {
        if (!IsTracked(ent)) return;
        if (!_toolSystem.HasQuality(args.Used, ent.Comp.QualityToRemove)) return;
        args.Handled = true;
        if (_electrocutionSystem.TryDoElectrocution(args.User, ent, 5, TimeSpan.FromSeconds(5), false)) return;
        var doafterArgs = new DoAfterArgs(EntityManager, args.User, ent.Comp.RemovalTime, new RemoveTrackerEvent(), ent.Owner)
        {
            BreakOnMove = true,
            BreakOnDamage = true,
            BreakOnWeightlessMove = false,
            NeedHand = true,
            DuplicateCondition = DuplicateConditions.SameEvent,
        };
        _doAfterSystem.TryStartDoAfter(doafterArgs);
    }

    private void OnRemoveTracker(Entity<GunTrackableComponent> ent, ref RemoveTrackerEvent args)
    {
        if (args.Cancelled || args.Handled) return;
        args.Handled = true;
        _itemSlots.TryEject(ent, ent.Comp.GunTrackerSlotId, args.User, out var _);
    }

    private void OnGunTrackableInit(Entity<GunTrackableComponent> ent, ref MapInitEvent args)
    {
        if (_itemSlots.TryGetSlot(ent.Owner, ent.Comp.GunTrackerSlotId, out _)) return;
        var whitelist = new EntityWhitelist
        {
            Components = new string[] { "GunTracker" },
        };
        _itemSlots.AddItemSlot(ent.Owner, ent.Comp.GunTrackerSlotId, new ItemSlot
        {
            Name = "Gun Tracker",
            Whitelist = whitelist,
            Swap = false,
            DisableEject = true,
        });
    }
}
