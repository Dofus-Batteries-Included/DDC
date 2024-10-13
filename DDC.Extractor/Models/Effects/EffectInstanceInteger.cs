namespace DDC.Extractor.Models.Effects;

public class EffectInstanceInteger : EffectInstance
{
    public EffectInstanceInteger(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceInteger instance) : base(instance)
    {
        Value = instance.value;
    }

    public int Value { get; set; }
}
