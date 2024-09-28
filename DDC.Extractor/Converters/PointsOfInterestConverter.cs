using Core.DataCenter.Metadata.Quest.TreasureHunt;

namespace DDC.Extractor.Converters;

public class PointsOfInterestConverter : IConverter<PointOfInterest, Models.PointOfInterest>
{
    public Models.PointOfInterest Convert(PointOfInterest data) =>
        new()
        {
            PoiId = data.id,
            NameId = data.nameId,
            CategoryId = data.categoryId
        };
}
