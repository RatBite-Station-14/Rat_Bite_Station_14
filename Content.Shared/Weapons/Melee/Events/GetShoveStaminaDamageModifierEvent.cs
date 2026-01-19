// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Weapons.Melee.Events;

/// <summary>
/// Raised on shover to allow systems/components to modify shove stamina damage.
/// </summary>
[ByRefEvent]
public struct GetShoveStaminaDamageModifierEvent
{
    public float Multiplier;

    public GetShoveStaminaDamageModifierEvent(float multiplier)
    {
        Multiplier = multiplier;
    }
}
