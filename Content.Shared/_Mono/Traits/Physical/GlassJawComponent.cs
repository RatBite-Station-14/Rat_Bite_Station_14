// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Decreases the damage threshold required to enter the Critical state.
/// </summary>
[RegisterComponent]
public sealed partial class GlassJawComponent : Component
{
    /// <summary>
    /// How much to decrease the Critical damage threshold by.
    /// </summary>
    [DataField]
    public int CritDecrease = 10;
}
