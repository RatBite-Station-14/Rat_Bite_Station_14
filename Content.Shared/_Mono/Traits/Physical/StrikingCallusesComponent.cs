// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Increases unarmed strike base damage by a flat amount.
/// </summary>
[RegisterComponent]
public sealed partial class StrikingCallusesComponent : Component
{
    /// <summary>
    /// Flat bonus to add to unarmed base damage (Blunt).
    /// </summary>
    [DataField]
    public int BluntBonus = 2;
}
