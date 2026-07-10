// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Applies the Hemophilia effects. Royal disease anyone?
/// </summary>
[RegisterComponent]
public sealed partial class HemophiliaComponent : Component
{
    /// <summary>
    /// Multiplier applied to BloodstreamComponent.
    /// </summary>
    [DataField]
    public float BleedReductionMultiplier = 0.5f;

    /// <summary>
    /// Multiplier applied to incoming Blunt damage.
    /// </summary>
    [DataField]
    public float BluntDamageMultiplier = 1.10f;

    /// <summary>
    /// Additional multiplier applied to bleed added from incoming damage. Cooked.
    /// </summary>
    [DataField]
    public float ExtraBleedOnDamageMultiplier = 1.0f;
}
