namespace DDC.Extractor.Models;

public class Area
{
    public int Id { get; init; }
    public int NameId { get; init; }
    public int? WorldMapId { get; init; }
    public int? SuperAreaId { get; init; }
    public Bounds Bounds { get; init; }
    public bool ContainHouses { get; init; }
    public bool ContainPaddocks { get; init; }
}
