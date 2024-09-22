using Core.DataCenter.Metadata.World;
using DDC.Extractor.Abstractions;
using DDC.Extractor.Areas.Models;

namespace DDC.Extractor.Areas;

public class SuperAreasConverter : IConverter<SuperAreas, SuperArea>
{
    public SuperArea Convert(SuperAreas data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            WorldMapId = data.hasWorldMap ? data.worldmapId : null
        };
}
