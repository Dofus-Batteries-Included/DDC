using Core.DataCenter.Metadata.Interactive;
using DDC.Extractor.Models;
using DDC.Extractor.Models.Worlds;

namespace DDC.Extractor.Converters;

public class InteractivesConverter : IConverter<Interactives, Interactive>
{
    public Interactive Convert(Interactives data) => new(data);
}
