// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Strip.Components;

/// <summary>
/// Marker for the character trait so its bonuses survive other thieving sources.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class ThievingTraitComponent : Component
{
    public const float StripTimeMultiplier = 0.6f;
}
