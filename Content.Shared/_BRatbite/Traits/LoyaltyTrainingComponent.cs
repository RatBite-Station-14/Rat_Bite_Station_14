// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class LoyaltyTrainingComponent : Component
{
    [DataField]
    public EntProtoId Implant = "MindShieldImplant";
}
