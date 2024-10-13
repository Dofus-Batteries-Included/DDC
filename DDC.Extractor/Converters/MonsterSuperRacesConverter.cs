using Core.DataCenter.Metadata.Monster;
using DDC.Extractor.Models.Monsters;

namespace DDC.Extractor.Converters;

public class MonsterSuperRacesConverter : IConverter<MonsterSuperRaces, MonsterSuperRace>
{
    public MonsterSuperRace Convert(MonsterSuperRaces data) => new(data);
}
