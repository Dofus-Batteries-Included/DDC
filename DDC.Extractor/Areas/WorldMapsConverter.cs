using Core.DataCenter.Metadata.World;
using DDC.Extractor.Abstractions;
using DDC.Extractor.Areas.Models;
using DDC.Extractor.Common.Models;

namespace DDC.Extractor.Areas;

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
