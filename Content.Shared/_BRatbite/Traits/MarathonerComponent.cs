// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class MarathonerComponent : Component
{
    [DataField]
    public float StaminaBonus = 25f;

    [DataField]
    public float WalkSpeedMultiplier = 1f;

    [DataField]
    public float SprintSpeedMultiplier = 1.05f;

    [DataField]
    public float StandUpTimeMultiplier = 0.85f;
}
