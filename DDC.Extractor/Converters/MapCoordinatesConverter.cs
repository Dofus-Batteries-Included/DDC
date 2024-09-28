using System.Collections.Generic;
using System.Linq;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class MapCoordinatesConverter : IConverter<Core.DataCenter.Metadata.World.MapCoordinates, Models.MapCoordinates>
{
    public Models.MapCoordinates Convert(Core.DataCenter.Metadata.World.MapCoordinates data) =>
        new()
        {
            Position = new Position
            {
                X = (int)Core.DataCenter.Metadata.World.MapCoordinates.GetSignedValue(data.x),
                Y = (int)Core.DataCenter.Metadata.World.MapCoordinates.GetSignedValue(data.y)
            },
            MapIds = new List<long>(data.mapIds._items.Take(data.mapIds.Count))
        };
}
