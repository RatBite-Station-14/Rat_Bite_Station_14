// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Linq;
using Content.Goobstation.Maths.FixedPoint;
using Content.Shared._BRatbite.Traits;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Nyanotrasen.Item.PseudoItem;
using Robust.Server.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Collision.Shapes;
using Robust.Shared.Physics.Systems;

namespace Content.Server._BRatbite.Traits;

public sealed class SmallTraitSystem : EntitySystem
{
    [Dependency] private readonly AppearanceSystem _appearance = default!;
    [Dependency] private readonly MobThresholdSystem _mobThresholds = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<SmallTraitComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<SmallTraitComponent> ent, ref ComponentStartup args)
    {
        ApplyScale(ent);
        ApplyPseudoItem(ent);
        ApplyThresholds(ent.Owner, ent.Comp);
    }

    private void ApplyScale(Entity<SmallTraitComponent> ent)
    {
        EnsureComp<ScaleVisualsComponent>(ent);

        var appearance = EnsureComp<AppearanceComponent>(ent);
        _appearance.SetData(ent, ScaleVisuals.Scale, ent.Comp.VisualScale, appearance);

        if (!TryComp(ent, out FixturesComponent? fixtures))
            return;

        foreach (var (id, fixture) in fixtures.Fixtures)
        {
            if (fixture.Shape is not PhysShapeCircle circle)
                continue;

            _physics.SetPositionRadius(
                ent,
                id,
                fixture,
                circle,
                circle.Position,
                ent.Comp.CollisionRadius,
                fixtures);
        }
    }

    private void ApplyPseudoItem(Entity<SmallTraitComponent> ent)
    {
        var pseudoItem = EnsureComp<PseudoItemComponent>(ent);
        pseudoItem.Size = ent.Comp.Size;
        pseudoItem.StoredOffset = ent.Comp.StoredOffset;
        pseudoItem.Shape = ent.Comp.Shape;
        Dirty(ent, pseudoItem);
    }

    private void ApplyThresholds(EntityUid uid, SmallTraitComponent comp, MobThresholdsComponent? thresholds = null)
    {
        if (comp.AppliedThresholds || !Resolve(uid, ref thresholds, false))
            return;

        var current = thresholds.Thresholds.ToArray();
        foreach (var (threshold, state) in current)
        {
            if (state == MobState.Alive)
                continue;

            _mobThresholds.SetMobStateThreshold(
                uid,
                FixedPoint2.Max((FixedPoint2)1, threshold * comp.HealthMultiplier),
                state,
                thresholds);
        }

        comp.AppliedThresholds = true;
    }
}
