namespace DDC.Extractor.Models.Effects;

public class EffectInstanceDuration : EffectInstance
{
    public EffectInstanceDuration(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDuration instance) : base(instance)
    {
        Days = instance.days;
        Hours = instance.hours;
        Minutes = instance.minutes;
    }

    public int Days { get; set; }
    public int Hours { get; set; }
    public int Minutes { get; set; }
}
