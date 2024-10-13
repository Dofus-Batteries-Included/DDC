using Core.DataCenter.Metadata.Quest.TreasureHunt;

namespace DDC.Extractor.Converters;

public class PointsOfInterestConverter : IConverter<PointOfInterest, Models.Worlds.PointOfInterest>
{
    public Models.Worlds.PointOfInterest Convert(PointOfInterest data) => new(data);
}
