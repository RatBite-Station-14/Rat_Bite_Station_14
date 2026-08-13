using System.Numerics;
using Content.Server.Popups;
using Content.Server.Silicons.StationAi;
using Content.Shared._BRatbite.Silicon;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Components;
using Content.Shared._Shitmed.Medical.Surgery.Traumas.Systems;
using Content.Shared._Shitmed.Medical.Surgery.Wounds.Systems;
using Content.Shared.Body.Components;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

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
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly TraumaSystem _trauma = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly WoundSystem _wound = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoreRollActionEvent>(OnRollAction);
        SubscribeLocalEvent<CoreRollerComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CoreRollerComponent, CoreRollDoAfterEvent>(OnRollDoAfter);
    }

    private void OnMapInit(Entity<CoreRollerComponent> core, ref MapInitEvent args)
    {
        if (HasComp<StationAiCoreComponent>(core))
            RemComp<PullableComponent>(core);
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

        if (IsBlocked(core.Owner, destination))
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
        if (IsBlocked(core.Owner, destination))
            return;

        foreach (var target in _lookup.GetEntitiesInRange(mapDestination, 0.45f, LookupFlags.Dynamic))
        {
            if (target == core.Owner || Transform(target).Anchored || !HasComp<DamageableComponent>(target))
                continue;

            _damageable.TryChangeDamage(
                target,
                new DamageSpecifier(_prototypes.Index<DamageTypePrototype>("Blunt"), core.Comp.CrushDamage),
                origin: core.Owner);
            CrushBody(target, core.Comp.BonesToBreak, core.Comp.SquishScale, core.Comp.SquishDuration);
        }

        _transform.SetCoordinates(
            (core.Owner, Transform(core.Owner), MetaData(core.Owner)),
            destination,
            rotation: Angle.Zero);
        _audio.PlayPvs(RollSound, core.Owner);
        _popup.PopupCoordinates(Loc.GetString("core-roll-thud"), Transform(core.Owner).Coordinates, PopupType.Large);
        args.Handled = true;
    }

    private void CrushBody(EntityUid target, int bonesToBreak, Vector2 squishScale, TimeSpan squishDuration)
    {
        if (!TryComp(target, out BodyComponent? body) ||
            body.RootContainer.ContainedEntities.Count == 0)
        {
            return;
        }

        if (!HasComp<CoreSquishedComponent>(target))
        {
            AddComp(target, new CoreSquishedComponent
            {
                SquishScale = squishScale,
                RecoveryStart = _timing.CurTime,
                RecoveryDuration = squishDuration,
            });
        }

        if (bonesToBreak <= 0)
            return;

        var bones = new List<Entity<BoneComponent>>();
        var root = body.RootContainer.ContainedEntities[0];
        foreach (var woundable in _wound.GetAllWoundableChildren(root))
        {
            foreach (var bone in woundable.Comp.Bone.ContainedEntities)
            {
                if (TryComp(bone, out BoneComponent? boneComp) && boneComp.BoneIntegrity > 0)
                    bones.Add((bone, boneComp));
            }
        }

        _random.Shuffle(bones);
        for (var i = 0; i < Math.Min(bonesToBreak, bones.Count); i++)
        {
            var bone = bones[i];
            _trauma.SetBoneIntegrity(bone.Owner, 0, bone.Comp);
        }
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<CoreSquishedComponent>();
        while (query.MoveNext(out var uid, out var squished))
        {
            if (_timing.CurTime >= squished.RecoveryStart + squished.RecoveryDuration)
                RemCompDeferred<CoreSquishedComponent>(uid);
        }
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

    private bool IsBlocked(EntityUid core, EntityCoordinates destination)
    {
        var tile = _turf.GetTileRef(destination);
        if (tile == null || tile.Value.Tile.IsEmpty || !TryComp(core, out PhysicsComponent? corePhysics))
            return true;

        foreach (var target in _lookup.GetEntitiesInRange(destination, 0.45f, LookupFlags.Static | LookupFlags.Dynamic))
        {
            if (target == core ||
                !Transform(target).Anchored ||
                !TryComp(target, out PhysicsComponent? targetPhysics) ||
                !targetPhysics.CanCollide ||
                !targetPhysics.Hard)
            {
                continue;
            }

            if ((targetPhysics.CollisionMask & corePhysics.CollisionLayer) != 0 ||
                (targetPhysics.CollisionLayer & corePhysics.CollisionMask) != 0)
            {
                return true;
            }
        }

        return false;
    }
}
