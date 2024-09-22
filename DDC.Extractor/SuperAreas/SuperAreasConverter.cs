using DDC.Extractor.Abstractions;
using DDC.Extractor.SuperAreas.Models;

namespace DDC.Extractor.SuperAreas;

public class SuperAreasConverter : IConverter<Core.DataCenter.Metadata.World.SuperAreas, SuperArea>
{
    public SuperArea Convert(Core.DataCenter.Metadata.World.SuperAreas data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            WorldMapId = data.hasWorldMap ? data.worldmapId : null
        };
}
