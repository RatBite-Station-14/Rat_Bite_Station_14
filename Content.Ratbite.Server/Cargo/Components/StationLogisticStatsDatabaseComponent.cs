// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Trauma.Common.CartridgeLoader.Cartridges;

namespace Content.Ratbite.Server.Cargo.Components;

/// <summary>
/// Added to the abstract representation of a station to track stats related to mail delivery and income
/// </summary>
[RegisterComponent]
public sealed partial class StationLogisticStatsComponent : Component
{
    [DataField]
    public MailStats Metrics { get; set; }
}
