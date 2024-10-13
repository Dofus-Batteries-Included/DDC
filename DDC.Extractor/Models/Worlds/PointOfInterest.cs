namespace DDC.Extractor.Models.Worlds;

public class PointOfInterest
{
    public PointOfInterest(Core.DataCenter.Metadata.Quest.TreasureHunt.PointOfInterest data)
    {
        PoiId = data.id;
        NameId = data.nameId;
        CategoryId = data.categoryId;
    }

    public int PoiId { get; init; }
    public int NameId { get; init; }
    public int CategoryId { get; init; }
}
