// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Shared._BRatbite.Traits;

using Content.Shared.Inventory;
using Content.Shared.Body.Part;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee;

public sealed class PaciFistSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;
    [Dependency] private readonly UnarmedCombatSkillSystem _unarmedCombat = default!;

    public bool IsBarehandWeapon(EntityUid user, EntityUid? weapon)
    {
        if (weapon == null)
            return HasComp<MeleeWeaponComponent>(user);

        if (weapon.Value == user)
            return true;

        return _inventory.TryGetSlotEntity(user, "gloves", out var gloves) &&
               gloves == weapon.Value;
    }

    public bool TryGetActivePaciFist(EntityUid user, EntityUid? weapon, out PaciFistComponent? paciFist)
    {
        paciFist = null;

        if (!IsBarehandWeapon(user, weapon) ||
            _unarmedCombat.IsUnarmedCombatSkillBlocked(user) ||
            !TryComp(user, out paciFist))
        {
            return false;
        }

        return true;
    }

    public bool CanPaciFistAttackMob(EntityUid user, EntityUid? weapon, EntityUid target, out PaciFistComponent? paciFist)
    {
        return TryGetPaciFistMobTarget(user, weapon, target, out paciFist, out _);
    }

    public bool IsMobTarget(EntityUid target)
    {
        return TryGetMobTarget(target, out _);
    }

    public bool TryGetPaciFistMobTarget(
        EntityUid user,
        EntityUid? weapon,
        EntityUid target,
        out PaciFistComponent? paciFist,
        out EntityUid mobTarget)
    {
        paciFist = null;
        mobTarget = default;

        return TryGetMobTarget(target, out mobTarget) &&
               TryGetActivePaciFist(user, weapon, out paciFist);
    }

    public bool TryGetMobTarget(EntityUid target, out EntityUid mobTarget)
    {
        if (HasComp<MobStateComponent>(target))
        {
            mobTarget = target;
            return true;
        }

        if (TryComp<BodyPartComponent>(target, out var bodyPart) &&
            bodyPart.Body is { } body &&
            HasComp<MobStateComponent>(body))
        {
            mobTarget = body;
            return true;
        }

        mobTarget = default;
        return false;
    }
}
