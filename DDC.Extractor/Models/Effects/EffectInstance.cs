using Core.DataCenter.Metadata.Effect;
using DDC.Extractor.Models.Spells;
using Metadata.Enums;

namespace DDC.Extractor.Models.Effects;

public class EffectInstance
{
    public EffectInstance(Core.DataCenter.Metadata.Effect.EffectInstance instance)
    {
        EffectId = instance.effectId;
        EffectUid = instance.effectUid;
        BaseEffectId = instance.baseEffectId;
        EffectElement = instance.effectElement;
        Delay = instance.delay;
        Duration = instance.duration;
        Dispellable = instance.dispellable;
        Group = instance.group;
        Modificator = instance.modificator;
        Order = instance.order;
        Trigger = instance.trigger;
        Triggers = instance.triggers;
        ShowInSet = instance.showInSet;
        Random = instance.random;
        SpellId = instance.spellId;
        TargetId = instance.targetId;
        TargetMask = instance.targetMask;
        ZoneStopAtTarget = instance.zoneStopAtTarget;
        ZoneDescription = new SpellZoneDescription(instance.zoneDescr);
        ZoneShape = instance.zoneShape;
        ZoneSize = instance.zoneSize;
        ZoneMinSize = instance.zoneMinSize;
        ZoneDamageDecreaseStepPercent = instance.zoneDamageDecreaseStepPercent;
        ZoneMaxDamageDecreaseApplyCount = instance.zoneMaxDamageDecreaseApplyCount;
        VisibleInTooltip = instance.visibleInTooltip;
        VisibleOnTerrain = instance.visibleOnTerrain;
        VisibleInBuffUi = instance.visibleInBuffUi;
        VisibleInFightLog = instance.visibleInFightLog;
        ForClientOnly = instance.forClientOnly;
    }

    public ActionId EffectId { get; set; }
    public int EffectUid { get; set; }
    public short BaseEffectId { get; set; }
    public sbyte EffectElement { get; set; }
    public byte Delay { get; set; }
    public sbyte Duration { get; set; }
    public byte Dispellable { get; set; }
    public short Group { get; set; }
    public short Modificator { get; set; }
    public int Order { get; set; }
    public bool Trigger { get; set; }
    public string Triggers { get; set; }
    public bool ShowInSet { get; set; }
    public float Random { get; set; }
    public short SpellId { get; set; }
    public short TargetId { get; set; }
    public string TargetMask { get; set; }
    public bool ZoneStopAtTarget { get; set; }
    public SpellZoneDescription ZoneDescription { get; set; }
    public SpellZoneShape ZoneShape { get; set; }
    public byte ZoneSize { get; set; }
    public byte ZoneMinSize { get; set; }
    public int ZoneDamageDecreaseStepPercent { get; set; }
    public int ZoneMaxDamageDecreaseApplyCount { get; set; }
    public bool VisibleInTooltip { get; set; }
    public bool VisibleOnTerrain { get; set; }
    public bool VisibleInBuffUi { get; set; }
    public bool VisibleInFightLog { get; set; }
    public bool ForClientOnly { get; set; }
}

static class EffectInstanceMappingExtensions
{
    public static EffectInstance ToInstance(this Core.DataCenter.Metadata.Effect.EffectInstance instance) =>
        instance switch
        {
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceString str => new EffectInstanceString(str),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDice dice => new EffectInstanceDice(dice),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceInteger integer => new EffectInstanceInteger(integer),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceMinMax minMax => new EffectInstanceMinMax(minMax),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDate date => new EffectInstanceDate(date),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDuration duration => new EffectInstanceDuration(duration),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceLadder ladder => new EffectInstanceLadder(ladder),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceMount mount => new EffectInstanceMount(mount),
            Core.DataCenter.Metadata.Effect.Instance.EffectInstanceCreature creature => new EffectInstanceCreature(creature),
            _ => new EffectInstance(instance)
        };
}
