// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Damage;
using Robust.Shared.GameStates;

namespace Content.Shared._Lavaland.Weapons.Marker;

/// <summary>
/// Applies leech upon hitting a damage marker target.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class DamageBoostOnMarkerComponent : Component
{
    [DataField(required: true)]
    public DamageSpecifier Boost = new();

    [DataField]
    public DamageSpecifier? BackstabBoost;
}