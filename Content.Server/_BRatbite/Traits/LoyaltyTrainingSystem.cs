// SPDX-FileCopyrightText: 2026 Sprinkle <40203084+lnn0q@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._BRatbite.Traits;
using Content.Shared.Implants;
using Content.Shared.Mindshield.Components;

namespace Content.Server._BRatbite.Traits;

public sealed class LoyaltyTrainingSystem : EntitySystem
{
    [Dependency] private readonly SharedSubdermalImplantSystem _implants = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<LoyaltyTrainingComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<LoyaltyTrainingComponent> ent, ref ComponentStartup args)
    {
        if (HasComp<MindShieldComponent>(ent))
            return;

        _implants.AddImplant(ent, ent.Comp.Implant);
    }
}
