// SPDX-FileCopyrightText: 2026 Perstronzio Desantis <44839463+PropenzioLavandino@users.noreply.github.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

namespace Content.Goobstation.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Destructible.Thresholds.Behaviors;
using Content.Server.Radio.EntitySystems;
using Content.Server.Destructible;
using Content.Shared.IdentityManagement;

/// <summary>
/// Sends a message to a selected radio channel
/// </summary>
[DataDefinition]
public sealed partial class RadioBehavior : IThresholdBehavior
{
    /// <summary>
    /// Locale id of the radio message.
    /// </summary>
    [DataField("radioMessage", required: true)]
    public string radioMessage;

    /// <summary>
    /// Which radio channel to show
    /// </summary>
    [DataField("radioChannel", required: true)]
    public string radioChannel;

    public void Execute(EntityUid uid, DestructibleSystem system, EntityUid? cause = null)
    {
	var radio = system.EntityManager.System<RadioSystem>();
	 radio.SendRadioMessage(uid,
				Loc.GetString(radioMessage, ("entityName", Identity.Name(uid, system.EntityManager))),
				radioChannel,
				uid);
    }
}
