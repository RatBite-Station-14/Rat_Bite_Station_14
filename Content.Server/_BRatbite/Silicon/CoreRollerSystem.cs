using System.Numerics;
using Content.Server.Popups;
using Content.Server.Silicons.StationAi;
using Content.Shared._BRatbite.Silicon;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.DoAfter;
using Content.Shared.Maps;
using Content.Shared.Mind;
using Content.Shared.Mind.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Physics;
using Content.Shared.Popups;
using Content.Shared.Silicons.StationAi;
using Robust.Server.Audio;
using Robust.Shared.Audio;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Prototypes;

namespace Content.Server._BRatbite.Silicon;

public sealed class CoreRollerSystem : EntitySystem
{
    private static readonly SoundSpecifier RollSound = new SoundPathSpecifier("/Audio/Effects/Footsteps/largethud.ogg");

    [Dependency] private readonly AudioSystem _audio = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;
    [Dependency] private readonly StationAiSystem _stationAi = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly TurfSystem _turf = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<CoreRollActionEvent>(OnRollAction);
        SubscribeLocalEvent<ToggleAiCoreControlActionEvent>(OnToggleAiCoreControl);
        SubscribeLocalEvent<CoreRollerComponent, CoreRollDoAfterEvent>(OnRollDoAfter);
        SubscribeLocalEvent<CoreRollerComponent, MoveInputEvent>(OnMoveInput);
        SubscribeLocalEvent<CoreRollerComponent, MindUnvisitedMessage>(OnMindUnvisited);
        SubscribeLocalEvent<CoreRollerComponent, ComponentShutdown>(OnShutdown);
    }

    private void OnRollAction(CoreRollActionEvent args)
    {
        if (args.Handled || !TryGetCore(args.Performer, out var core))
            return;

        var origin = _transform.GetMapCoordinates(core.Owner);
        var clicked = _transform.ToMapCoordinates(args.Target);
        if (origin.MapId != clicked.MapId)
            return;

        var offset = clicked.Position - origin.Position;
        if (offset.LengthSquared() < 0.01f)
            return;

        var direction = MathF.Abs(offset.X) > MathF.Abs(offset.Y)
            ? new Vector2(MathF.Sign(offset.X), 0f)
            : new Vector2(0f, MathF.Sign(offset.Y));
        StartRoll(core, args.Performer, direction);
        args.Handled = true;
    }

    private void OnToggleAiCoreControl(ToggleAiCoreControlActionEvent args)
    {
        if (args.Handled)
            return;

        if (TryComp(args.Performer, out VisitingMindComponent? visiting) &&
            HasComp<CoreRollerComponent>(args.Performer) &&
            visiting.MindId is { } visitingMind)
        {
            _mind.UnVisit(visitingMind);
            args.Handled = true;
            return;
        }

        if (!HasComp<StationAiHeldComponent>(args.Performer) ||
            !_stationAi.TryGetCore(args.Performer, out var aiCore) ||
            !HasComp<CoreRollerComponent>(aiCore.Owner) ||
            !_mind.TryGetMind(args.Performer, out var mindId, out var mind) ||
            mind.IsVisitingEntity)
            return;

        _mind.Visit(mindId, aiCore.Owner, mind);
        if (TryComp(aiCore.Owner, out CoreRollerComponent? roller))
            _actions.AddAction(aiCore.Owner, ref roller.ControlActionEntity, roller.ControlAction);

        args.Handled = true;
    }

    private void OnMindUnvisited(Entity<CoreRollerComponent> core, ref MindUnvisitedMessage args)
    {
        _actions.RemoveAction(core.Owner, core.Comp.ControlActionEntity);
        core.Comp.ControlActionEntity = null;
    }

    private void OnShutdown(Entity<CoreRollerComponent> core, ref ComponentShutdown args)
    {
        _actions.RemoveAction(core.Owner, core.Comp.ControlActionEntity);
        core.Comp.ControlActionEntity = null;
    }

    private void OnMoveInput(Entity<CoreRollerComponent> core, ref MoveInputEvent args)
    {
        if (!args.State || !HasComp<VisitingMindComponent>(core.Owner))
            return;

        StartRoll(core, core.Owner, args.Dir.ToVec());
    }

    private void StartRoll(Entity<CoreRollerComponent> core, EntityUid performer, Vector2 direction)
    {
        if (direction.LengthSquared() < 0.01f)
            return;

        var origin = _transform.GetMapCoordinates(core.Owner);
        var cardinal = MathF.Abs(direction.X) > MathF.Abs(direction.Y)
            ? new Vector2(MathF.Sign(direction.X), 0f)
            : new Vector2(0f, MathF.Sign(direction.Y));
        var mapDestination = new MapCoordinates(origin.Position + cardinal, origin.MapId);
        var destination = _transform.ToCoordinates(mapDestination);

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
        }

        _transform.SetMapCoordinates(core.Owner, mapDestination);
        _transform.AttachToGridOrMap(core.Owner, Transform(core.Owner));
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

    private bool IsBlocked(EntityUid core, EntityCoordinates destination)
    {
        var tile = _turf.GetTileRef(destination);
        if (tile == null || tile.Value.Tile.IsEmpty || _turf.IsTileBlocked(tile.Value, CollisionGroup.MobMask))
            return true;

        foreach (var target in _lookup.GetEntitiesInRange(destination, 0.4f, LookupFlags.Static | LookupFlags.Dynamic))
        {
            if (target == core)
                continue;

            if (Transform(target).Anchored && HasComp<FixturesComponent>(target))
                return true;
        }

        return false;
    }
}
