// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Goobstation.Shared.MartialArts.Components;

/// <summary>
/// Component for Tide Style ability action entities
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class TideStyleAbilityComponent : Component
{
    [DataField(required: true)]
    public TideStyleAbility Configuration;

    [DataField]
    public string Name = string.Empty;
}
