// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.DoAfter;

namespace Content.Shared._BRatbite.Cuffs;

[RegisterComponent]
public sealed partial class TemporaryCuffsComponent : Component
{
    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan MinimumInterruptedStruggleTime = TimeSpan.FromSeconds(2);

    public DoAfterId? BreakoutDoAfter;
}
