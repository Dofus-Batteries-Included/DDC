namespace DDC.Extractor.Models.Effects;

public class EffectInstanceMinMax : EffectInstance
{
    public EffectInstanceMinMax(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceMinMax instance) : base(instance)
    {
        Min = instance.min;
        Max = instance.max;
    }

    public int Min { get; set; }
    public int Max { get; set; }
}
