using System.Collections.Generic;
using System.Linq;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Monsters;

public class Monster
{
    public Monster(Core.DataCenter.Metadata.Monster.Monsters monster)
    {
        Id = monster.id;
        NameId = monster.nameId;
        GfxId = monster.gfxId;
        Race = monster.race;
        Grades = monster.grades.ToCSharpList().Select(g => new MonsterGrade(g)).ToArray();
        Look = monster.look;
        Drops = monster.drops.ToCSharpList().Select(d => new MonsterDrop(d)).ToArray();
        TemporisDrops = monster.temporisDrops.ToCSharpList().Select(d => new MonsterDrop(d)).ToArray();
        Subareas = monster.subareas.ToCSharpList();
        Spells = monster.spells.ToCSharpList();
        SpellGrades = monster.spellGrades.ToCSharpList();
        FavoriteSubareaId = monster.favoriteSubareaId;
        CorrespondingMiniBossId = monster.correspondingMiniBossId;
        SpeedAdjust = monster.speedAdjust;
        CreatureBoneId = monster.creatureBoneId;
        IncompatibleIdols = monster.incompatibleIdols.ToCSharpList();
        IncompatibleChallenges = monster.incompatibleChallenges.ToCSharpList();
        AggressiveZoneSize = monster.aggressiveZoneSize;
        AggressiveLevelDiff = monster.aggressiveLevelDiff;
        AggressiveImmunityCriterion = monster.aggressiveImmunityCriterion;
        AggressiveAttackDelay = monster.aggressiveAttackDelay;
        ScaleGradeRef = monster.scaleGradeRef;
        CharacRatios = monster.characRatios.ToCSharpList().Select(cl => cl.values.ToCSharpList()).ToArray();
    }

    public int Id { get; set; }
    public int NameId { get; set; }
    public ushort GfxId { get; set; }
    public short Race { get; set; }
    public IReadOnlyList<MonsterGrade> Grades { get; set; }
    public string Look { get; set; }
    public IReadOnlyList<MonsterDrop> Drops { get; set; }
    public IReadOnlyList<MonsterDrop> TemporisDrops { get; set; }
    public IReadOnlyList<uint> Subareas { get; set; }
    public IReadOnlyList<int> Spells { get; set; }
    public IReadOnlyList<string> SpellGrades { get; set; }
    public ushort FavoriteSubareaId { get; set; }
    public ushort CorrespondingMiniBossId { get; set; }
    public sbyte SpeedAdjust { get; set; }
    public sbyte CreatureBoneId { get; set; }
    public IReadOnlyList<uint> IncompatibleIdols { get; set; }
    public IReadOnlyList<uint> IncompatibleChallenges { get; set; }
    public byte AggressiveZoneSize { get; set; }
    public short AggressiveLevelDiff { get; set; }
    public string AggressiveImmunityCriterion { get; set; }
    public short AggressiveAttackDelay { get; set; }
    public byte ScaleGradeRef { get; set; }
    public IReadOnlyList<IReadOnlyList<float>> CharacRatios { get; set; }
}
