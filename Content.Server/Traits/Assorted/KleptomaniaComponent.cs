// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using System.Numerics;

namespace Content.Server.Traits.Assorted;

[RegisterComponent, Access(typeof(KleptomaniaSystem))]
public sealed partial class KleptomaniaComponent : Component
{
    [DataField]
    public Vector2 TimeBetweenIncidents = new(12f, 20f);

    [DataField]
    public float Range = 1.5f;

    [DataField]
    public float FloorItemCooldown = 10f;

    [DataField]
    public bool RiskyTargets;

    public float NextIncidentTime;
}
