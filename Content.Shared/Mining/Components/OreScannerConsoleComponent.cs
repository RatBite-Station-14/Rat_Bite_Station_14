// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.GameStates;

namespace Content.Shared.Mining.Components;

/// <summary>
/// Console displaying ore locations on a rader-like ui (indirect vgroid/asteroid buff), it just works.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class SharedOreScannerConsoleComponent : Component
{
    [DataField, AutoNetworkedField]
    public float MaxRange = 512f;

    [DataField]
    public float UpdateRate = 1f;

    [DataField]
    public TimeSpan LastUpdate = TimeSpan.Zero;
}
