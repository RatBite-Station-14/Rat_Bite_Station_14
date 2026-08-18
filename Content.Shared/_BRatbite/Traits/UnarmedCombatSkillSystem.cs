// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Goobstation.Common.MartialArts;
using Content.Shared.Inventory;

namespace Content.Shared._BRatbite.Traits;

public sealed class UnarmedCombatSkillSystem : EntitySystem
{
    [Dependency] private readonly InventorySystem _inventory = default!;

    public bool IsUnarmedCombatSkillBlocked(EntityUid user)
    {
        if (HasComp<MartialArtsKnowledgeComponent>(user) ||
            HasComponentByName(user, "KravMaga"))
            return true;

        return _inventory.TryGetSlotEntity(user, "gloves", out var gloves) &&
               HasComp<UnarmedCombatSkillBlockerComponent>(gloves);
    }

    private bool HasComponentByName(EntityUid user, string componentName)
    {
        return EntityManager.ComponentFactory.TryGetRegistration(componentName, out var registration) &&
               EntityManager.HasComponent(user, registration.Type);
    }
}
