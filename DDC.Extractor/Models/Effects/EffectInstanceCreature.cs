namespace DDC.Extractor.Models.Effects;

public class EffectInstanceCreature : EffectInstance
{
    public EffectInstanceCreature(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceCreature instance) : base(instance)
    {
        MonsterFamilyId = instance.monsterFamilyId;
    }

    public short MonsterFamilyId { get; set; }
}
