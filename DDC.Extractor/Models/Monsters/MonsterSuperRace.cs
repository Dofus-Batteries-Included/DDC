using Core.DataCenter.Metadata.Monster;

namespace DDC.Extractor.Models.Monsters;

public class MonsterSuperRace
{
    public MonsterSuperRace(MonsterSuperRaces superRaces)
    {
        Id = superRaces.id;
        NameId = superRaces.nameId;
    }

    public int Id { get; set; }
    public int NameId { get; set; }
}
