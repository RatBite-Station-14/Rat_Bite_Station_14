// SPDX-FileCopyrightText: 2024 Angelo Fallaria <ba.fallaria@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Traits.Assorted.Components;

/// <summary>
///     Modifies footstep volume for traits like Light Step.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class FootstepVolumeModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float SprintVolumeModifier;

    [DataField, AutoNetworkedField]
    public float WalkVolumeModifier;
}
