// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class NeurogenesisImperfectaBrainComponent : Component
{
    [DataField]
    public EntityUid? OriginalBody;
}
