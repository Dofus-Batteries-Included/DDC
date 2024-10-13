using Core.DataCenter.Metadata.World;
using DDC.Extractor.Models;
using DDC.Extractor.Models.Worlds;

namespace DDC.Extractor.Converters;

public class AreasConverter : IConverter<Areas, Area>
{
    public Area Convert(Areas data) => new(data);
}
