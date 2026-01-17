// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Content.Shared.Mining.Components;

namespace Content.Shared.Mining.Systems;

/// <summary>
/// Shared ore scanner console system
/// </summary>
public abstract class SharedOreScannerConsoleSystem : EntitySystem
{
    public const float DefaultMaxRange = 512f;

    public virtual void SetRange(EntityUid uid, float value, SharedOreScannerConsoleComponent? component = null)
    {
        if (!Resolve(uid, ref component))
            return;

        if (component.MaxRange.Equals(value))
            return;

        component.MaxRange = value;
        Dirty(uid, component);
    }
}
