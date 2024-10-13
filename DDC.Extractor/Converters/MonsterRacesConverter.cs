using Core.DataCenter.Metadata.Monster;
using DDC.Extractor.Models.Monsters;

namespace DDC.Extractor.Converters;

public class MonsterRacesConverter : IConverter<MonsterRaces, MonsterRace>
{
    public MonsterRace Convert(MonsterRaces data) => new(data);
}
