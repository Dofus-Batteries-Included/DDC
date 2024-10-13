using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Models.Worlds;

public class WorldMap
{
    public WorldMap(WorldMaps data)
    {
        Id = data.id;
        NameId = data.nameId;
        Origin = new Position { X = data.origineX, Y = data.origineY };
    }

    public int Id { get; set; }
    public int NameId { get; set; }
    public Position Origin { get; set; }
}
