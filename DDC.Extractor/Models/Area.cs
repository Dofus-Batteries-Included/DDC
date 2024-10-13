using Core.DataCenter.Metadata.World;

namespace DDC.Extractor.Models;

public class Area
{
    public Area(Areas area)
    {
        Id = area.id;
        NameId = area.nameId;
        WorldMapId = area.hasWorldMap ? area.worldmapId : null;
        SuperAreaId = area.superAreaId;
        Bounds = area.bounds.ToBounds();
        ContainHouses = area.containHouses;
        ContainPaddocks = area.containPaddocks;
    }

    public int Id { get; init; }
    public int NameId { get; init; }
    public int? WorldMapId { get; init; }
    public int? SuperAreaId { get; init; }
    public Bounds Bounds { get; init; }
    public bool ContainHouses { get; init; }
    public bool ContainPaddocks { get; init; }
}
