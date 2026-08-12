using System.Numerics;
using Content.Server.Popups;
using Content.Server.Silicons.StationAi;
using Content.Shared._BRatbite.Silicon;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Silicon;

public sealed class CoreRollerSystem : EntitySystem
{
    private static readonly SoundSpecifier RollSound = new SoundPathSpecifier("/Audio/Effects/Footsteps/largethud.ogg");

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoreRollActionEvent>(OnRollAction);
        SubscribeLocalEvent<CoreRollerComponent, CoreRollDoAfterEvent>(OnRollDoAfter);
    }

    private void OnRollAction(CoreRollActionEvent args)
    {
        if (args.Handled || !TryGetCore(args.Performer, out var core))
            return;

        var xform = Transform(core.Owner);
        if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? grid))
            return;

        var origin = _transform.GetMapCoordinates(core.Owner);
        var clicked = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != clicked.MapId)
            return;

        var originTile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var clickedTile = _map.TileIndicesFor(gridUid, grid, clicked);
        var offset = clickedTile - originTile;
        if (offset == Vector2i.Zero)
            return;

        var direction = Math.Abs(offset.X) > Math.Abs(offset.Y)
            ? new Vector2i(Math.Sign(offset.X), 0)
            : new Vector2i(0, Math.Sign(offset.Y));
        StartRoll(core, args.Performer, direction);
        args.Handled = true;
    }

    private void StartRoll(Entity<CoreRollerComponent> core, EntityUid performer, Vector2i direction)
    {
        if (direction == Vector2i.Zero)
            return;

        var xform = Transform(core.Owner);
        if (xform.GridUid is not { } gridUid || !TryComp(gridUid, out MapGridComponent? grid))
            return;

        var originTile = _map.TileIndicesFor(gridUid, grid, xform.Coordinates);
        var destination = _map.GridTileToLocal(gridUid, grid, originTile + direction);

        if (IsBlocked(destination))
        {
            _popup.PopupEntity(Loc.GetString("core-roll-blocked"), core.Owner, performer);
            return;
        }

        _popup.PopupCoordinates(Loc.GetString("core-roll-warning", ("core", core.Owner)), Transform(core.Owner).Coordinates, PopupType.LargeCaution);

        var doAfter = new DoAfterArgs(
            EntityManager,
            performer,
            core.Comp.RollDelay,
            new CoreRollDoAfterEvent(GetNetCoordinates(destination)),
            core.Owner)
        {
            BreakOnDamage = true,
            BreakOnMove = true,
            NeedHand = false,
            DuplicateCondition = DuplicateConditions.SameEvent,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnRollDoAfter(Entity<CoreRollerComponent> core, ref CoreRollDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        var destination = GetCoordinates(args.Destination);
        var mapDestination = _transform.ToMapCoordinates(destination);
        if (IsBlocked(destination))
            return;

        foreach (var target in _lookup.GetEntitiesInRange(mapDestination, 0.45f, LookupFlags.Dynamic))
        {
            if (target == core.Owner || Transform(target).Anchored || !HasComp<DamageableComponent>(target))
                continue;

            _damageable.TryChangeDamage(
                target,
                new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Blunt"), core.Comp.CrushDamage),
                origin: core.Owner);
        }

        _transform.SetCoordinates(core.Owner, destination);
        _audio.PlayPvs(RollSound, core.Owner);
        _popup.PopupCoordinates(Loc.GetString("core-roll-thud"), Transform(core.Owner).Coordinates, PopupType.Large);
        args.Handled = true;
    }

    private bool TryGetCore(EntityUid performer, out Entity<CoreRollerComponent> core)
    {
        if (TryComp(performer, out CoreRollerComponent? direct))
        {
            core = (performer, direct);
            return true;
        }

        if (HasComp<StationAiHeldComponent>(performer) &&
            _stationAi.TryGetCore(performer, out var aiCore) &&
            TryComp(aiCore.Owner, out CoreRollerComponent? aiRoller))
        {
            core = (aiCore.Owner, aiRoller);
            return true;
        }

        core = default;
        return false;
    }

    private bool IsBlocked(EntityCoordinates destination)
    {
        var tile = _turf.GetTileRef(destination);
        return tile == null ||
               tile.Value.Tile.IsEmpty ||
               _turf.IsTileBlocked(tile.Value, CollisionGroup.MobMask);
    }
}
