// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Shared.MartialArts.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.GameObjects;

namespace Content.Goobstation.Shared.MartialArts;

/// <summary>
/// TideStyle doubles normal shove stamina damage.
/// </summary>
public sealed class TideStyleShoveModifierSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<TideStyleComponent, GetShoveStaminaDamageModifierEvent>(OnGetMod);
    }

    private void OnGetMod(EntityUid uid, TideStyleComponent comp, ref GetShoveStaminaDamageModifierEvent args)
    {
        args.Multiplier *= 2f;
    }
}
