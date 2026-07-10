// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted;

/// <summary>
/// Reduces the visual intensity of the drunk effect.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class AlcoholToleranceComponent : Component
{
    /// <summary>
    /// Multiplier applied to drunk visual intensity.
    /// </summary>
    [DataField]
    public float VisualScaleMultiplier = 0.35f;
}
