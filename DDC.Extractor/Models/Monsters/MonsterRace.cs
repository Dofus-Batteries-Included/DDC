using System.Collections.Generic;
using Core.DataCenter.Metadata.Monster;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Monsters;

public class MonsterRace
{
    public MonsterRace(MonsterRaces race)
    {
        Id = race.id;
        SuperRaceId = race.superRaceId;
        NameId = race.nameId;
        AggressiveZoneSize = race.aggressiveZoneSize;
        AggressiveLevelDiff = race.aggressiveLevelDiff;
        AggressiveImmunityCriterion = race.aggressiveImmunityCriterion;
        AggressiveAttackDelay = race.aggressiveAttackDelay;
        Monsters = race.monsters.ToCSharpList();
    }

    public int Id { get; set; }
    public int SuperRaceId { get; set; }
    public int NameId { get; set; }
    public int AggressiveZoneSize { get; set; }
    public int AggressiveLevelDiff { get; set; }
    public string AggressiveImmunityCriterion { get; set; }
    public int AggressiveAttackDelay { get; set; }
    public IReadOnlyList<uint> Monsters { get; set; }
}
