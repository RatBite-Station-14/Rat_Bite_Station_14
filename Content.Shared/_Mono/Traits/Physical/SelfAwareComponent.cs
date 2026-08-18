// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared._Mono.Traits.Physical;

/// <summary>
/// Adds a self-examine verb for the Self-Aware trait.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class SelfAwareComponent : Component;
