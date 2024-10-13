namespace DDC.Extractor.Models.Effects;

public class EffectInstanceLadder : EffectInstanceCreature
{
    public EffectInstanceLadder(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceLadder instance) : base(instance)
    {
        MonsterCount = instance.monsterCount;
    }

    public int MonsterCount { get; set; }
}
