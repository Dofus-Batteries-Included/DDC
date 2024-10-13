using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;
using DDC.Extractor.Models.Worlds;

namespace DDC.Extractor.Converters;

public class SubAreasConverter : IConverter<SubAreas, SubArea>
{
    public SubArea Convert(SubAreas data) => new(data);
}
