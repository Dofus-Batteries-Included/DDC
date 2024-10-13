using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Converters;

public class MapCoordinatesConverter : IConverter<MapCoordinates, Models.Worlds.MapCoordinates>
{
    public Models.Worlds.MapCoordinates Convert(MapCoordinates data) => new(data);
}
