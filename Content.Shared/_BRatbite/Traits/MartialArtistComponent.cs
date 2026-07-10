// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

[RegisterComponent]
public sealed partial class MartialArtistComponent : Component
{
    [DataField]
    public float DamageMultiplier = 1.2f;

    [DataField]
    public float AttackRateMultiplier = 1.25f;

    [DataField]
    public float RangeMultiplier = 1.1f;
}
