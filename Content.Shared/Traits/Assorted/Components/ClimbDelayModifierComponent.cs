// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     Modifies climb do-after duration for traits like Parkour Training.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class ClimbDelayModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float ClimbDelayMultiplier = 1f;
}
