using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class WorldMapsConverter : IConverter<WorldMaps, WorldMap>
{
    public WorldMap Convert(WorldMaps data) => new(data);
}
