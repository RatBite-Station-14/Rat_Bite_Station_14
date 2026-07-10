// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Marks that this entity should have bionic arms on spawn.
/// </summary>
[RegisterComponent]
public sealed partial class PrybarProstheticsComponent : Component
{
    [DataField]
    public float PrySpeedModifier = 1f;

    [DataField]
    public bool PryPowered;
}
