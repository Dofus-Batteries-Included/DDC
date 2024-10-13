using System.Collections.Generic;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models;

public class MapCoordinates
{
    public MapCoordinates(Core.DataCenter.Metadata.World.MapCoordinates data)
    {
        Position = new Position
        {
            X = (int)Core.DataCenter.Metadata.World.MapCoordinates.GetSignedValue(data.x),
            Y = (int)Core.DataCenter.Metadata.World.MapCoordinates.GetSignedValue(data.y)
        };
        MapIds = data.mapIds.ToCSharpList();
    }

    public Position Position { get; init; }
    public IReadOnlyCollection<long> MapIds { get; init; }
}
