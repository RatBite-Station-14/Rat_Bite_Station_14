// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.PermaBrig;

[RegisterComponent, NetworkedComponent]
public sealed partial class PermaUplinkComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionOpenPermaUplink";

    [DataField]
    public EntityUid? ActionEntity;
}
