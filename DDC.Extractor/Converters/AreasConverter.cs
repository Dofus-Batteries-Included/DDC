using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class AreasConverter : IConverter<Areas, Area>
{
    public Area Convert(Areas data) => new(data);
}
