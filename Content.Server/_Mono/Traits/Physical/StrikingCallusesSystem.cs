// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._Mono.Traits.Physical;
using Content.Shared.Damage;
using Content.Shared.Weapons.Melee.Events;
using Content.Goobstation.Maths.FixedPoint;

namespace Content.Server._Mono.Traits.Physical;

/// <summary>
/// Applies the Striking Calluses bonus to unarmed melee damage.
/// </summary>
public sealed class StrikingCallusesSystem : EntitySystem
{
    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<StrikingCallusesComponent, GetMeleeDamageEvent>(OnGetMeleeDamage);
    }

    private void OnGetMeleeDamage(Entity<StrikingCallusesComponent> ent, ref GetMeleeDamageEvent args)
    {
        if (args.User != args.Weapon)
            return;

        if (ent.Comp.BluntBonus <= 0)
            return;

        var bonus = new DamageSpecifier();
        bonus.DamageDict.Add("Blunt", FixedPoint2.New(ent.Comp.BluntBonus));
        args.Damage += bonus;
    }
}
