// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._Mono.Traits.Physical;

[RegisterComponent]
public sealed partial class BionicLegsComponent : Component
{
    [DataField]
    public float? WalkSpeed;

    [DataField]
    public float? SprintSpeed;
}
