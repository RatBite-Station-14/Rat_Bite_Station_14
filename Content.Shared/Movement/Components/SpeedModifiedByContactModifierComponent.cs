// SPDX-FileCopyrightText: 2024 SlamBamActionman <83650252+slambamactionman@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Skubman <ba.fallaria@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Movement.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared.Movement.Components;

/// <summary>
///     Modifies how strongly contact slowdown affects this entity.
/// </summary>
[NetworkedComponent, RegisterComponent, AutoGenerateComponentState]
[Access(typeof(SpeedModifierContactsSystem))]
public sealed partial class SpeedModifiedByContactModifierComponent : Component
{
    [DataField, AutoNetworkedField]
    public float WalkModifierEffectiveness = 1f;

    [DataField, AutoNetworkedField]
    public float SprintModifierEffectiveness = 1f;
}
