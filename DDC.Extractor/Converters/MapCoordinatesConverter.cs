using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Converters;

public class MapCoordinatesConverter : IConverter<MapCoordinates, Models.MapCoordinates>
{
    public Models.MapCoordinates Convert(MapCoordinates data) => new(data);
}
