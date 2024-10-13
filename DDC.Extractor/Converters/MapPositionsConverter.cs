using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;
using DDC.Extractor.Models.Worlds;

namespace DDC.Extractor.Converters;

public class MapPositionsConverter : IConverter<MapPositions, MapPosition>
{
    public MapPosition Convert(MapPositions data) => new(data);
}
