using Core.DataCenter.Metadata.Monster;
using DDC.Extractor.Models.Monsters;

namespace DDC.Extractor.Converters;

public class MonstersConverter : IConverter<Monsters, Monster>
{
    public Monster Convert(Monsters data) => new(data);
}
