// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

using Robust.Shared.GameStates;

[RegisterComponent, NetworkedComponent]
public sealed partial class PaciFistComponent : Component
{
    [DataField]
    public float StaminaDamage = 18f;

    [DataField]
    public float ShovePowerMultiplier = 0.75f;

    // The client needs this for prediction so pacifist Paci-fist attacks do not
    // run the normal harm feedback path before the server correction arrives.
    public override bool SendOnlyToOwner => true;
}
