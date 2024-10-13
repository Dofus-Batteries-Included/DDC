using Core.DataCenter.Metadata.Effect;
using DDC.Extractor.Models.Spells;
using Metadata.Enums;

namespace DDC.Extractor.Models.Effects;

public class EffectInstance
{
    public ActionId EffectId { get; set; }
    public int EffectUid { get; set; }
    public short BaseEffectId { get; set; }
    public sbyte EffectElement { get; set; }
    public int OppositeId { get; set; }
    public int Category { get; set; }
    public string Description { get; set; }
    public string TheoreticalDescription { get; set; }
    public string TheoreticalShortDescriptionForTooltip { get; set; }
    public byte Delay { get; set; }
    public sbyte Duration { get; set; }
    public string DurationString { get; set; }
    public byte Dispellable { get; set; }
    public short Group { get; set; }
    public short Modificator { get; set; }
    public int Order { get; set; }
    public int Priority { get; set; }
    public bool Trigger { get; set; }
    public string Triggers { get; set; }
    public int EffectTriggerDuration { get; set; }
    public string CharacteristicOperator { get; set; }
    public bool IsInPercent { get; set; }
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
    public bool HideValueInTooltip { get; set; }
    public bool VisibleOnTerrain { get; set; }
    public bool VisibleInBuffUi { get; set; }
    public bool VisibleInFightLog { get; set; }
    public bool ForClientOnly { get; set; }
    public bool UseInFight { get; set; }
    public int TextIconReferenceId { get; set; }
}

static class EffectInstanceMappingExtensions
{
    public static EffectInstance ToInstance(this Core.DataCenter.Metadata.Effect.EffectInstance instance) =>
        new()
        {
            EffectId = instance.effectId,
            EffectUid = instance.effectUid,
            BaseEffectId = instance.baseEffectId,
            EffectElement = instance.effectElement,
            OppositeId = instance.oppositeId,
            Category = instance.category,
            Description = instance.description,
            TheoreticalDescription = instance.theoreticalDescription,
            TheoreticalShortDescriptionForTooltip = instance.theoreticalShortDescriptionForTooltip,
            Delay = instance.delay,
            Duration = instance.duration,
            DurationString = instance.durationString,
            Dispellable = instance.dispellable,
            Group = instance.group,
            Modificator = instance.modificator,
            Order = instance.order,
            Priority = instance.priority,
            Trigger = instance.trigger,
            Triggers = instance.triggers,
            EffectTriggerDuration = instance.effectTriggerDuration,
            CharacteristicOperator = instance.characteristicOperator,
            IsInPercent = instance.isInPercent,
            ShowInSet = instance.showInSet,
            Random = instance.random,
            SpellId = instance.spellId,
            TargetId = instance.targetId,
            TargetMask = instance.targetMask,
            ZoneStopAtTarget = instance.zoneStopAtTarget,
            ZoneDescription = instance.zoneDescr.ToDescription(),
            ZoneShape = instance.zoneShape,
            ZoneSize = instance.zoneSize,
            ZoneMinSize = instance.zoneMinSize,
            ZoneDamageDecreaseStepPercent = instance.zoneDamageDecreaseStepPercent,
            ZoneMaxDamageDecreaseApplyCount = instance.zoneMaxDamageDecreaseApplyCount,
            VisibleInTooltip = instance.visibleInTooltip,
            HideValueInTooltip = instance.hideValueInTooltip,
            VisibleOnTerrain = instance.visibleOnTerrain,
            VisibleInBuffUi = instance.visibleInBuffUi,
            VisibleInFightLog = instance.visibleInFightLog,
            ForClientOnly = instance.forClientOnly,
            UseInFight = instance.useInFight,
            TextIconReferenceId = instance.textIconReferenceId
        };
}
