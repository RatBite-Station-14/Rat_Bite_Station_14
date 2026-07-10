// SPDX-FileCopyrightText: 2022 Leon Friedrich <60421075+ElectroJr@users.noreply.github.com>
// SPDX-FileCopyrightText: 2022 keronshb <54602815+keronshb@users.noreply.github.com>
// SPDX-FileCopyrightText: 2023 DrSmugleaf <DrSmugleaf@users.noreply.github.com>
// SPDX-FileCopyrightText: 2024 Krunklehorn <42424291+Krunklehorn@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Aiden <28298836+Aidenkrz@users.noreply.github.com>
// SPDX-FileCopyrightText: 2025 Monolith Station contributors
//
// SPDX-License-Identifier: MIT

using Content.Shared.Alert;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared.Strip.Components;

/// <summary>
/// Give this to an entity when you want to decrease stripping times
/// </summary>
[RegisterComponent, NetworkedComponent]
[AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class ThievingComponent : Component
{
    /// <summary>
    /// How much the strip time should be shortened by
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan StripTimeReduction = TimeSpan.FromSeconds(0.5f);

    /// <summary>
    /// Should it notify the user if they're stripping a pocket?
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Stealthy;

    /// <summary>
    /// Reduces strip warning popups to small non-caution text without hiding the do-after.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool Subtle;

    /// <summary>
    /// Toggles <see cref="Subtle"/> instead of <see cref="Stealthy"/> when the alert is clicked.
    /// </summary>
    [DataField]
    public bool ToggleSubtle;

    /// <summary>
    /// Multiplies the final strip time.
    /// </summary>
    [DataField, AutoNetworkedField]
    public float TimeMultiplier = 1f;

    /// <summary>
    /// Allows seeing strip slots that are normally hidden.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool IdentifyHidden;

    /// <summary>
    /// Marks thieving granted by the character trait, as opposed to thief role or equipment sources.
    /// </summary>
    [DataField, AutoNetworkedField]
    public bool TraitGranted;

    /// <summary>
    /// Variable pointing at the Alert modal
    /// </summary>
    [DataField]
    public ProtoId<AlertPrototype> StealthyAlertProtoId = "Stealthy";

    /// <summary>
    /// Prevent component replication to clients other than the owner,
    /// doesn't affect prediction.
    /// Get mogged.
    /// </summary>
    public override bool SendOnlyToOwner => true;
}

/// <summary>
/// Event raised to toggle the thieving component.
/// </summary>
public sealed partial class ToggleThievingEvent : BaseAlertEvent;
