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
using Robust.Shared.Map;
using Content.Shared.Prying.Components;

namespace Content.Server._Mono.Traits.Physical;

/// <summary>
/// Handles replacing arms with JWL arms on spawn for entities with PrybarProstheticsComponent.
/// </summary>
public sealed class PrybarProstheticsSystem : EntitySystem
{
    private const string LeftArmSlot = "left arm";
    private const string RightArmSlot = "right arm";
    private const string LeftHandSlot = "left hand";
    private const string RightHandSlot = "right hand";

    private static readonly EntProtoId JawsOfLifeLeftArm = "JawsOfLifeLeftArm";
    private static readonly EntProtoId JawsOfLifeRightArm = "JawsOfLifeRightArm";
    private static readonly EntProtoId LeftHandCybernetic = "LeftHandCybernetic";
    private static readonly EntProtoId RightHandCybernetic = "RightHandCybernetic";

    [Dependency] private readonly SharedBodySystem _bodySystem = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;
    [Dependency] private readonly SharedContainerSystem _containerSystem = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<PrybarProstheticsComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<PrybarProstheticsComponent> ent, ref ComponentStartup args)
    {
        ReplaceArms(ent);
        ApplyPryingOverrides(ent);
    }

    private void ReplaceArms(Entity<PrybarProstheticsComponent> ent)
    {
        if (!TryComp(ent, out BodyComponent? body))
            return;

        if (body.RootContainer.ContainedEntity is not { } torso)
            return;

        if (!TryComp(torso, out BodyPartComponent? torsoPart))
            return;

        ReplacePartIfPresent(torso, torsoPart, LeftArmSlot, JawsOfLifeLeftArm, LeftHandSlot, LeftHandCybernetic);
        ReplacePartIfPresent(torso, torsoPart, RightArmSlot, JawsOfLifeRightArm, RightHandSlot, RightHandCybernetic);
    }

    private void ApplyPryingOverrides(Entity<PrybarProstheticsComponent> ent)
    {
        if (!TryComp<PryingComponent>(ent, out var prying))
            return;

        prying.SpeedModifier = ent.Comp.PrySpeedModifier;
        prying.PryPowered = ent.Comp.PryPowered;
        Dirty(ent.Owner, prying);
    }

    private void ReplacePartIfPresent(
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

        AttachChildPart(newPart, newPartComp, childSlotId, childProtoId);
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
                    {
                        DeleteChildParts(child, childPart);
                    }

                    _containerSystem.Remove(child, childContainer);

                    QueueDel(child);
                }
            }
        }
    }
}
