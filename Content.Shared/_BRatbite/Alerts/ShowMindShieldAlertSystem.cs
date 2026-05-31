// SPDX-FileCopyrightText: 2026 Perstronzio Desantis <44839463+PropenzioLavandino@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Alert;
using Content.Shared.Mindshield.Components;

namespace Content.Shared._BRatbite.Alerts;

public sealed class ShowMindShieldAlertSystem : EntitySystem {
    [Dependency] private readonly AlertsSystem _alerts = default!;
	public override void Initialize()
	{
	    base.Initialize();

	    SubscribeLocalEvent<MindShieldComponent, ComponentStartup>(OnMindShieldStartup);
	    SubscribeLocalEvent<MindShieldComponent, ComponentShutdown>(OnMindShieldShutdown);
	}

	private void OnMindShieldStartup(Entity<MindShieldComponent> ent, ref ComponentStartup args)
	{

	    _alerts.ShowAlert(ent, ent.Comp.MindShieldAlert);
	}

	private void OnMindShieldShutdown(Entity<MindShieldComponent> ent, ref ComponentShutdown args)
	{
	    _alerts.ClearAlert(ent, ent.Comp.MindShieldAlert);
	}
}
