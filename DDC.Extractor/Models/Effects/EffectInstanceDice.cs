namespace DDC.Extractor.Models.Effects;

public class EffectInstanceDice : EffectInstanceInteger
{
    public EffectInstanceDice(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDice instance) : base(instance)
    {
        DiceNum = instance.diceNum;
        DiceSide = instance.diceSide;
        DisplayZero = instance.displayZero;
    }

    public int DiceNum { get; set; }
    public int DiceSide { get; set; }
    public bool DisplayZero { get; set; }
}
