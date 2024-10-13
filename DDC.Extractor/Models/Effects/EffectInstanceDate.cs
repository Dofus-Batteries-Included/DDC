namespace DDC.Extractor.Models.Effects;

public class EffectInstanceDate : EffectInstance
{
    public EffectInstanceDate(Core.DataCenter.Metadata.Effect.Instance.EffectInstanceDate instance) : base(instance)
    {
        Year = instance.year;
        Month = instance.month;
        Day = instance.day;
        Hour = instance.hour;
        Minute = instance.minute;
    }

    public int Year { get; set; }
    public int Day { get; set; }
    public int Month { get; set; }
    public int Hour { get; set; }
    public int Minute { get; set; }
}
