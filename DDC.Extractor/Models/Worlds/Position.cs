using Core.DataCenter.Types;

namespace DDC.Extractor.Models.Worlds;

public class Position
{
    public int X { get; init; }
    public int Y { get; init; }
}

static class PositionMappingExtensions
{
    public static Position ToPosition(this Point point) =>
        new()
        {
            X = point.x,
            Y = point.y
        };

    public static Position ToNonEmptyPositionOrNull(this Point point)
    {
        if (point.x == 0 && point.y == 0)
        {
            return null;
        }

        return point.ToPosition();
    }
}
