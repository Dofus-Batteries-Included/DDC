using System.Collections.Generic;
using System.Linq;
using Core.DataCenter.Metadata.Spell;
using Metadata.Enums;

namespace DDC.Extractor.Models.Spells;

public class SpellZoneDescription
{
    public SpellZoneDescription(SpellZoneDescr descr)
    {
        CellIds = descr.cellIds._items.Take(descr.cellIds.Count).ToArray();
        Shape = descr.shape;
        Param1 = descr.param1;
        Param2 = descr.param2;
        DamageDecreaseStepPercent = descr.damageDecreaseStepPercent;
        MaxDamageDecreaseApplyCount = descr.maxDamageDecreaseApplyCount;
        IsStopAtTarget = descr.isStopAtTarget;
    }

    public IReadOnlyList<int> CellIds { get; set; }
    public SpellZoneShape Shape { get; set; }
    public byte Param1 { get; set; }
    public byte Param2 { get; set; }
    public sbyte DamageDecreaseStepPercent { get; set; }
    public sbyte MaxDamageDecreaseApplyCount { get; set; }
    public bool IsStopAtTarget { get; set; }
}
