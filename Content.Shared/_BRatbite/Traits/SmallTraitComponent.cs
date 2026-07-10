// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;
using Content.Shared.Item;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class SmallTraitComponent : Component
{
    [DataField]
    public Vector2 VisualScale = new(0.65f, 0.65f);

    [DataField]
    public float CollisionRadius = 0.2f;

    [DataField]
    public float HealthMultiplier = 0.3f;

    [DataField]
    public Vector2i StoredOffset = new(0, 17);

    [DataField]
    public List<Box2i> Shape = new()
    {
        new Box2i(0, 0, 3, 3),
    };

    [DataField]
    public ProtoId<ItemSizePrototype> Size = "Huge";

    [DataField]
    public bool AppliedThresholds;
}
