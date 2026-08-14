// SPDX-FileCopyrightText: 2026 Perstronzio Desantis <44839463+PropenzioLavandino@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared._BRatbite.Revolutionary;
using Content.Shared.Alert;
using Content.Shared.Mindshield.Components;

namespace Content.Shared._BRatbite.Alerts;

public sealed class ShowMindShieldAlertSystem : EntitySystem
{
    [Dependency] private readonly AlertsSystem _alerts = default!;
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MindShieldComponent, MindShieldChangedEvent>(OnMindShieldStartup);
    }

    private void OnMindShieldStartup(Entity<MindShieldComponent> ent, ref MindShieldChangedEvent args)
    {
        if (args.Fake) return;
        if (args.Active)
            _alerts.ShowAlert(ent.Owner, ent.Comp.MindShieldAlert);
        else
            _alerts.ClearAlert(ent.Owner, ent.Comp.MindShieldAlert);
    }
}
