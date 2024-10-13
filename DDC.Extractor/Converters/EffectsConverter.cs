using Core.DataCenter.Metadata.Effect;
using DDC.Extractor.Models.Effects;

namespace DDC.Extractor.Converters;

public class EffectsConverter : IConverter<Effects, Effect>
{
    public Effect Convert(Effects effect) => new(effect);
}
