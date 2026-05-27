// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Body.Components;
using Content.Shared.Body.Part;
using Content.Shared.Body.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;
using System.Linq;
using System.Numerics;
using Content.Shared._Mono.Traits.Physical;
using Content.Shared.Movement.Components;
using Robust.Shared.Map;
using Content.Shared.Standing;

namespace Content.Server._Mono.Traits.Physical;

public sealed class BionicLegsSystem : EntitySystem
{
    private const string LeftLegSlot = "left leg";
    private const string RightLegSlot = "right leg";
    private const string LeftFootSlot = "left foot";
    private const string RightFootSlot = "right foot";

    private static readonly EntProtoId SpeedLeftLeg = "SpeedLeftLeg";
    private static readonly EntProtoId SpeedRightLeg = "SpeedRightLeg";
    private static readonly EntProtoId LeftFootCybernetic = "LeftFootCybernetic";
    private static readonly EntProtoId RightFootCybernetic = "RightFootCybernetic";

    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;
    [Dependency] private readonly StandingStateSystem _standing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<BionicLegsComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<BionicLegsComponent> ent, ref ComponentStartup args)
    {
        ReplaceLegs(ent);
        _standing.Stand(ent.Owner, force: true);
    }

    private void ReplaceLegs(Entity<BionicLegsComponent> ent)
    {
        if (!TryComp(ent, out BodyComponent? body))
            return;

        if (body.RootContainer.ContainedEntities.Count == 0)
            return;

        var torso = body.RootContainer.ContainedEntities.FirstOrDefault();

        if (!TryComp(torso, out BodyPartComponent? torsoPart))
            return;

        if (TryFindPartWithSlot(torso, torsoPart, LeftLegSlot, out var leftLegParent, out var leftLegParentPart))
            ReplacePartIfPresent(ent.Comp, leftLegParent, leftLegParentPart, LeftLegSlot, SpeedLeftLeg, LeftFootSlot, LeftFootCybernetic);

        if (TryFindPartWithSlot(torso, torsoPart, RightLegSlot, out var rightLegParent, out var rightLegParentPart))
            ReplacePartIfPresent(ent.Comp, rightLegParent, rightLegParentPart, RightLegSlot, SpeedRightLeg, RightFootSlot, RightFootCybernetic);

        _bodySystem.UpdateMovementSpeed(ent.Owner, body);
    }

    private bool TryFindPartWithSlot(
        EntityUid parentEntity,
        BodyPartComponent parentPart,
        string slotId,
        out EntityUid foundEntity,
        out BodyPartComponent foundPart)
    {
        if (parentPart.Children.ContainsKey(slotId))
        {
            foundEntity = parentEntity;
            foundPart = parentPart;
            return true;
        }

        foreach (var (childSlotId, _) in parentPart.Children)
        {
            var childContainerId = SharedBodySystem.GetPartSlotContainerId(childSlotId);

            if (!_containerSystem.TryGetContainer(parentEntity, childContainerId, out var childContainer))
                continue;

            foreach (var child in childContainer.ContainedEntities)
            {
                if (!TryComp(child, out BodyPartComponent? childPart))
                    continue;

                if (TryFindPartWithSlot(child, childPart, slotId, out foundEntity, out foundPart))
                    return true;
            }
        }

        foundEntity = default;
        foundPart = default!;
        return false;
    }

    private void ReplacePartIfPresent(
        BionicLegsComponent component,
        EntityUid parentEntity,
        BodyPartComponent parentPart,
        string slotId,
        EntProtoId partProtoId,
        string childSlotId,
        EntProtoId childProtoId)
    {
        var containerId = SharedBodySystem.GetPartSlotContainerId(slotId);
        if (!_containerSystem.TryGetContainer(parentEntity, containerId, out var container) ||
            container.ContainedEntities.Count == 0)
            return;

        if (!_prototypeManager.TryIndex(partProtoId, out _))
            return;

        if (!_prototypeManager.TryIndex(childProtoId, out _))
            return;

        var oldEntities = container.ContainedEntities.ToArray();
        foreach (var oldEntity in oldEntities)
        {
            if (TryComp(oldEntity, out BodyPartComponent? oldPart))
                DeleteChildParts(oldEntity, oldPart);
        }

        foreach (var entity in oldEntities)
        {
            _containerSystem.Remove(entity, container);
            QueueDel(entity);
        }

        var newPart = Spawn(partProtoId, new EntityCoordinates(parentEntity, Vector2.Zero));

        if (!TryComp(newPart, out BodyPartComponent? newPartComp))
        {
            QueueDel(newPart);
            return;
        }

        if (!_bodySystem.AttachPart(parentEntity, slotId, newPart, parentPart, newPartComp))
        {
            QueueDel(newPart);
            return;
        }

        ApplySpeedOverrides(newPart, component);
        AttachChildPart(newPart, newPartComp, childSlotId, childProtoId);
    }

    private void ApplySpeedOverrides(EntityUid leg, BionicLegsComponent component)
    {
        if (!TryComp<MovementBodyPartComponent>(leg, out var movement))
            return;

        if (component.WalkSpeed is { } walkSpeed)
            movement.WalkSpeed = walkSpeed;

        if (component.SprintSpeed is { } sprintSpeed)
            movement.SprintSpeed = sprintSpeed;

        Dirty(leg, movement);
    }

    private void AttachChildPart(EntityUid parentEntity, BodyPartComponent parentPart, string slotId, EntProtoId partProtoId)
    {
        var newPart = Spawn(partProtoId, new EntityCoordinates(parentEntity, Vector2.Zero));

        if (!TryComp(newPart, out BodyPartComponent? newPartComp))
        {
            QueueDel(newPart);
            return;
        }

        if (!_bodySystem.AttachPart(parentEntity, slotId, newPart, parentPart, newPartComp))
            QueueDel(newPart);
    }

    private void DeleteChildParts(EntityUid parent, BodyPartComponent part)
    {
        foreach (var (slotId, _) in part.Children)
        {
            var childContainerId = SharedBodySystem.GetPartSlotContainerId(slotId);

            if (_containerSystem.TryGetContainer(parent, childContainerId, out var childContainer))
            {
                var children = childContainer.ContainedEntities.ToArray();

                foreach (var child in children)
                {
                    if (TryComp(child, out BodyPartComponent? childPart))
                        DeleteChildParts(child, childPart);

                    _containerSystem.Remove(child, childContainer);
                    QueueDel(child);
                }
            }
        }
    }
}
