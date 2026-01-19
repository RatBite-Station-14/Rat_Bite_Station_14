// SPDX-FileCopyrightText: 2026 Mond-Mann <moonmanrreal@gmail.com>
//
// SPDX-License-Identifier: AGPL-3.0-or-later

using Robust.Client.UserInterface.Controls;
using System.Linq;
using System.Numerics;

namespace Content.Goobstation.Client.MartialArts;

/// <summary>
/// Radial container with rotation support.
/// </summary>
public sealed class RotatedRadialContainer : LayoutContainer
{
    private const float RadiusIncrement = 5f;

    // Match RadialContainer API
    public Vector2 AngularRange
    {
        get => _angularRange;
        set
        {
            var x = value.X;
            var y = value.Y;

            x = x > MathF.Tau ? x % MathF.Tau : x;
            y = y > MathF.Tau ? y % MathF.Tau : y;

            x = x < 0 ? MathF.Tau + x : x;
            y = y < 0 ? MathF.Tau + y : y;

            _angularRange = new Vector2(x, y);
        }
    }

    private Vector2 _angularRange = new(0f, MathF.Tau - float.Epsilon);

    public RAlignment RadialAlignment { get; set; } = RAlignment.Clockwise;

    public float InitialRadius { get; set; } = 100f;

    public float CalculatedRadius { get; private set; }

    public float InnerRadiusMultiplier { get; set; } = 0.5f;

    public float OuterRadiusMultiplier { get; set; } = 1.5f;

    public bool ReserveSpaceForHiddenChildren { get; set; } = true;

    public float Rotation { get; set; }

    protected override Vector2 ArrangeOverride(Vector2 finalSize)
    {
        var children = ReserveSpaceForHiddenChildren
            ? Children
            : Children.Where(x => x.Visible);

        var childCount = children.Count();
        if (childCount == 0)
            return base.ArrangeOverride(finalSize);

        CalculatedRadius = InitialRadius + (childCount * RadiusIncrement);

        var isAntiClockwise = RadialAlignment == RAlignment.AntiClockwise;

        var arc = AngularRange.Y - AngularRange.X;
        arc = arc < 0 ? MathF.Tau + arc : arc;
        arc = isAntiClockwise ? MathF.Tau - arc : arc;

        var childMod = MathHelper.CloseTo(arc, MathF.Tau, 0.01f) ? 0 : 1;

        var sepAngle = arc / (childCount - childMod);
        sepAngle *= isAntiClockwise ? -1f : 1f;

        var controlCenter = finalSize * 0.5f;

        const float baseAngleOffset = MathF.PI * 0.5f;
        var angleOffset = baseAngleOffset + Rotation;

        var query = children.Select((x, index) => (index, x));
        foreach (var (childIndex, child) in query)
        {
            var targetAngleOfChild = AngularRange.X + sepAngle * (childIndex + 0.5f) + angleOffset;

            var position = new Vector2(
                    MathF.Floor(CalculatedRadius * MathF.Cos(targetAngleOfChild)),
                    MathF.Floor(-CalculatedRadius * MathF.Sin(targetAngleOfChild))
                ) + controlCenter - child.DesiredSize * 0.5f + Position;

            SetPosition(child, position);

            if (child is Content.Client.UserInterface.Controls.IRadialMenuItemWithSector tb)
            {
                tb.AngleSectorFrom = sepAngle * childIndex;
                tb.AngleSectorTo   = sepAngle * (childIndex + 1);
                tb.AngleOffset     = angleOffset;

                tb.InnerRadius  = CalculatedRadius * InnerRadiusMultiplier;
                tb.OuterRadius  = CalculatedRadius * OuterRadiusMultiplier;
                tb.ParentCenter = controlCenter;
            }
        }

        return base.ArrangeOverride(finalSize);
    }

    public enum RAlignment : byte
    {
        Clockwise,
        AntiClockwise,
    }
}
