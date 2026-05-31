// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Cuffs;

[RegisterComponent]
public sealed partial class TemporaryCuffsComponent : Component
{
    [DataField]
    public TimeSpan Lifetime = TimeSpan.FromSeconds(30);

    [DataField]
    public TimeSpan MinimumInterruptedStruggleTime = TimeSpan.FromSeconds(2);
}
