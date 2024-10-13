namespace DDC.Extractor.Models.Effects;

public class EffectInstanceString : EffectInstance
{
    public EffectInstanceString(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceString instance) : base(instance)
    {
        Text = instance.text;
    }

    public string Text { get; set; }
}
