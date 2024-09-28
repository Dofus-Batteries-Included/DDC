using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class WorldMapsConverter : IConverter<WorldMaps, WorldMap>
{
    public WorldMap Convert(WorldMaps data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            Origin = new Position { X = data.origineX, Y = data.origineY }
        };
}
