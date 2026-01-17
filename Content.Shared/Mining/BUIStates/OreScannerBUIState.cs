// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Shared.Serialization;
using System.Numerics;

namespace Content.Shared.Mining.BUIStates;

/// <summary>
/// State containing ore locations and types for display
/// </summary>
[Serializable, NetSerializable]
public sealed class OreScannerInterfaceState
{
    public List<OreBlip> Ores = new();
    public Vector2 ConsolePosition;
    public Angle ConsoleAngle;
    public float MaxRange;
    public bool RotateWithEntity = true;
}

[Serializable, NetSerializable]
public sealed class OreBlip
{
    public Vector2 Position;
    public string OreType = string.Empty;
    public Color BlipColor;
}

[Serializable, NetSerializable]
public sealed class OreScannerBoundUserInterfaceState : BoundUserInterfaceState
{
    public OreScannerInterfaceState State;

    public OreScannerBoundUserInterfaceState(OreScannerInterfaceState state)
    {
        State = state;
    }
}

[Serializable, NetSerializable]
public enum OreScannerUiKey : byte
{
    Key
}
