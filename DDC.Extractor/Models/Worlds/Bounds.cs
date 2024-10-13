using Core.DataCenter.Types;

namespace DDC.Extractor.Models.Worlds;

public class Bounds
{
    public Position Position { get; init; }
    public Size Size { get; init; }
}

static class BoundsMappingExtensions
{
    public static Bounds ToBounds(this Rectangle rectangle) =>
        new()
        {
            Position = new Position
            {
                X = (int)rectangle.x,
                Y = (int)rectangle.y
            },
            Size = new Size
            {
                Width = (int)rectangle.width,
                Height = (int)rectangle.height
            }
        };

    public static Bounds ToNonEmptyBoundsOrNull(this Rectangle rectangle)
    {
        if (rectangle.width == 0 && rectangle.height == 0)
        {
            return null;
        }

        return rectangle.ToBounds();
    }
}
