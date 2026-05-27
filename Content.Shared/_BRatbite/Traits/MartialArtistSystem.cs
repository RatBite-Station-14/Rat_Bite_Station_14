// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.Weapons;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._BRatbite.Traits;

public sealed class MartialArtistSystem : EntitySystem
{
    [Dependency] private readonly UnarmedCombatSkillSystem _unarmedCombat = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MartialArtistComponent, GetUserMeleeDamageEvent>(OnGetUserMeleeDamage);
        SubscribeLocalEvent<MartialArtistComponent, GetMeleeAttackRateEvent>(OnGetMeleeAttackRate);
        SubscribeLocalEvent<MartialArtistComponent, GetLightAttackRangeEvent>(OnGetLightAttackRange);
    }

    private void OnGetUserMeleeDamage(Entity<MartialArtistComponent> ent, ref GetUserMeleeDamageEvent args)
    {
        if (args.Weapon != ent.Owner || _unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
            return;

        args.Damage *= ent.Comp.DamageMultiplier;
    }

    private void OnGetMeleeAttackRate(Entity<MartialArtistComponent> ent, ref GetMeleeAttackRateEvent args)
    {
        if (args.Weapon != ent.Owner || _unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
            return;

        args.Multipliers *= ent.Comp.AttackRateMultiplier;
    }

    private void OnGetLightAttackRange(Entity<MartialArtistComponent> ent, ref GetLightAttackRangeEvent args)
    {
        if (args.User != ent.Owner || _unarmedCombat.IsUnarmedCombatSkillBlocked(ent.Owner))
            return;

        args.Range *= ent.Comp.RangeMultiplier;
    }
}
