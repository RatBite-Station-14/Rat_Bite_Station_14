// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

/// <summary>
/// Applied to equipment that should disable bare-hand combat traits such as PCT Training while worn.
/// </summary>
[RegisterComponent]
public sealed partial class UnarmedCombatSkillBlockerComponent : Component;
