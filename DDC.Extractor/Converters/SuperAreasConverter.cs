using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class SuperAreasConverter : IConverter<SuperAreas, SuperArea>
{
    public SuperArea Convert(SuperAreas data) => new(data);
}
