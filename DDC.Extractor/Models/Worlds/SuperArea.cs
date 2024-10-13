using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Models.Worlds;

public class SuperArea
{
    public SuperArea(SuperAreas data)
    {
        Id = data.id;
        NameId = data.nameId;
        WorldMapId = data.hasWorldMap ? data.worldmapId : null;
    }

    public int Id { get; init; }
    public int NameId { get; init; }
    public int? WorldMapId { get; init; }
}
