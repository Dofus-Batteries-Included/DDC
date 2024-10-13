namespace DDC.Extractor.Models.Effects;

public class Effect
{
    public Effect(Core.DataCenter.Metadata.Effect.Effects effect)
    {
        Id = effect.id;
        IconId = effect.iconId;
        Active = effect.active;
        Boost = effect.boost;
        DescriptionId = effect.descriptionId;
        TheoreticalDescriptionId = effect.theoreticalDescriptionId;
        ElementId = effect.elementId;
        OppositeId = effect.oppositeId;
        BonusType = effect.bonusType;
        Category = effect.category;
        Characteristic = effect.characteristic;
        CharacteristicOperator = effect.characteristicOperator;
        TheoreticalPattern = effect.theoreticalPattern;
        UseDice = effect.useDice;
        UseInFight = effect.useInFight;
        EffectPriority = effect.effectPriority;
        EffectPowerRate = effect.effectPowerRate;
        EffectTriggerDuration = effect.effectTriggerDuration;
        ForceMinMax = effect.forceMinMax;
        IsInPercent = effect.isInPercent;
        ShowInSet = effect.showInSet;
        ShowInTooltip = effect.showInTooltip;
        HideValueInTooltip = effect.hideValueInTooltip;
        TextIconReferenceId = effect.textIconReferenceId;
    }

    public int Id { get; set; }
    public int IconId { get; set; }
    public bool Active { get; set; }
    public bool Boost { get; set; }
    public int DescriptionId { get; set; }
    public string TheoreticalDescriptionId { get; set; }
    public int ElementId { get; set; }
    public int OppositeId { get; set; }
    public int BonusType { get; set; }
    public int Category { get; set; }
    public int Characteristic { get; set; }
    public string CharacteristicOperator { get; set; }
    public int TheoreticalPattern { get; set; }
    public bool UseDice { get; set; }
    public bool UseInFight { get; set; }
    public int EffectPriority { get; set; }
    public float EffectPowerRate { get; set; }
    public int EffectTriggerDuration { get; set; }
    public bool ForceMinMax { get; set; }
    public bool IsInPercent { get; set; }
    public bool ShowInSet { get; set; }
    public bool ShowInTooltip { get; set; }
    public bool HideValueInTooltip { get; set; }
    public int TextIconReferenceId { get; set; }
}
