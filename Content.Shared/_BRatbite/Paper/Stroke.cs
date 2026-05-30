using System.Numerics;
using Robust.Shared.Serialization;
using Robust.Shared.GameStates;

namespace Content.Shared._BRatbite.Paper;

[Serializable, NetSerializable]
public sealed class PaperStroke
{
    public List<Vector2> Points = new();
    public Color Color;

    public PaperStroke(Color color)
    {
        Color = color;
    }

    public void AddPoint(Vector2 point)
    {
        Points.Add(point);
    }
}
