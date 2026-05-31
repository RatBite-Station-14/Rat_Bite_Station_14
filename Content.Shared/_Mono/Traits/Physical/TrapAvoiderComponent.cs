// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Step triggers will not activate when this entity steps on them.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TrapAvoiderComponent : Component;
