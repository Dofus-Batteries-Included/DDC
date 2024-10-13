using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class SubAreasConverter : IConverter<SubAreas, SubArea>
{
    public SubArea Convert(SubAreas data) => new(data);
}
