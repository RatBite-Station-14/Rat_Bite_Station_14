// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Slippery;

/// <summary>
///     Modifies slip stun duration on an entity.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SlippableModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ParalyzeTimeMultiplier = 1f;
}
