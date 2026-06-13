using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Dynamics.Joints;
using Robust.Shared.Physics.Systems;
using Content.Shared.DragDrop;
using Content.Shared.Physics;
using Content.Goobstation.Maths.FixedPoint;
using Robust.Shared.Timing;
using Content.Shared.Chemistry;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.Body.Components;
using Robust.Shared.GameObjects;
using Robust.Shared.Containers;
using Content.Shared.Verbs;
using Content.Shared.Hands;

namespace Content.Shared._BRatbite.Medical;

public sealed partial class IVSystem : EntitySystem
{
    [Dependency] private readonly SharedJointSystem _joints = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainers = default!;
    [Dependency] private readonly ReactiveSystem _reactiveSystem = default!;
    [Dependency] private readonly ItemSlotsSystem _itemSlots = default!;
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IEntityManager _entityManager = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<IVComponent, CanDropDraggedEvent>(OnCanDrop);
        SubscribeLocalEvent<IVComponent, CanDropTargetEvent>(OnCanDropOn);
        SubscribeLocalEvent<IVComponent, DragDropDraggedEvent>(OnDragDropDragged);
        SubscribeLocalEvent<IVComponent, EntInsertedIntoContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<IVComponent, EntRemovedFromContainerMessage>(OnContainerModified);
        SubscribeLocalEvent<IVComponent, GetVerbsEvent<AlternativeVerb>>(AddAlternativeVerbs);
        SubscribeLocalEvent<IVComponent, GotEquippedHandEvent>(OnGetEquipped);

    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);
        if (!_timing.IsFirstTimePredicted)
            return;
        foreach (var comp in EntityManager.EntityQuery<IVComponent>())
        {
            if (_timing.CurTime < comp.NextUpdate)
                continue;
            if (comp.AttachedEntity == null) continue;
            comp.NextUpdate = _timing.CurTime + comp.UpdateTime;

            TryInject((comp.Owner, comp));
        }
    }


    private bool TryInject(Entity<IVComponent> ent)
    {
        var target = ent.Comp.AttachedEntity;
        if (target == null) return false;
        if (_itemSlots.TryGetSlot(ent.Owner, ent.Comp.ItemSlotId, out var itemSlot))
        {
            var item = itemSlot.Item;
            if (item == null) return false;
            if (!_solutionContainers.TryGetSolution((EntityUid) item, ent.Comp.SolutionName, out var beakerSoln, out var beakerSolution) || beakerSolution.Volume == 0)
                return false;
            if (!_solutionContainers.TryGetInjectableSolution((EntityUid) target, out var targetSoln, out var targetSolution))
            {
                return false;
            }
            var transferAmount = FixedPoint2.Min(ent.Comp.InjectedUnitsPerUpdate, targetSolution.AvailableVolume);
            if (transferAmount <= 0)
                return false;
            var removedSolution = _solutionContainers.SplitSolution(beakerSoln.Value, transferAmount);
            if (!targetSolution.CanAddSolution(removedSolution))
                return false;
            _reactiveSystem.DoEntityReaction((EntityUid) target, removedSolution, ReactionMethod.Injection);
            _solutionContainers.TryAddSolution(targetSoln.Value, removedSolution);
            return true;
        }
        return false;
    }

    private void OnCanDrop(Entity<IVComponent> ent, ref CanDropDraggedEvent args)
    {
        args.CanDrop = true;
        args.Handled = true;
    }

    private void OnCanDropOn(Entity<IVComponent> ent, ref CanDropTargetEvent args)
    {
        args.CanDrop = HasComp<BloodstreamComponent>(args.Dragged);
        args.Handled = args.CanDrop;
    }


    private void OnDragDropDragged(Entity<IVComponent> ent, ref DragDropDraggedEvent args)
    {
        ChangeAttachedEntity(ent, args.Target);
    }

    private void ChangeAttachedEntity(Entity<IVComponent> ent, EntityUid? target)
    {
        ent.Comp.AttachedEntity = target;
        _joints.ClearJoints(ent);
        var jointComponent = EnsureComp<JointComponent>(ent);
        var visuals = EnsureComp<JointVisualsComponent>(ent);
        if (target != null)
        {
            visuals.Sprite = ent.Comp.LineSprite;
            visuals.Target = GetNetEntity(target);
            var joint = _joints.CreateDistanceJoint(ent, (EntityUid) target);
            joint.MinLength = 0;
            joint.MaxLength = ent.Comp.MaxDistance;
        }
        else
        {
            visuals.Target = null;
        }
        Dirty(ent.Owner, jointComponent);
    }

    private void OnContainerModified<T>(Entity<IVComponent> ent, ref T args) where T : ContainerModifiedMessage
    {
        if (TryComp<AppearanceComponent>(ent, out var appearance))
        {
            _appearance.QueueUpdate(ent, appearance);
        }
    }

    private void AddAlternativeVerbs(Entity<IVComponent> ent, ref GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanAccess || !args.CanInteract)
            return;

        if (ent.Comp.AttachedEntity != null)
        {
            args.Verbs.Add(new AlternativeVerb
            {
                Text = Loc.GetString("iv-unattach"),
                Act = () => ChangeAttachedEntity(ent, null),
            });
        }
    }

    private void OnGetEquipped(Entity<IVComponent> ent, ref GotEquippedHandEvent args)
    {
        if (_timing.ApplyingState)
            return;
        ChangeAttachedEntity(ent, null);
        _entityManager.RemoveComponent<JointComponent>(ent.Owner);
        _entityManager.RemoveComponent<JointVisualsComponent>(ent.Owner);
    }
}
