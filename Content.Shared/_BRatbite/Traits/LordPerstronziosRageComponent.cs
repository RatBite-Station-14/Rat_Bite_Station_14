// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent, NetworkedComponent]
public sealed partial class LordPerstronziosRageComponent : Component
{
    [DataField]
    public EntProtoId Action = "ActionMald";

    [DataField]
    public EntityUid? ActionEntity;
}

public sealed partial class MaldActionEvent : InstantActionEvent;
