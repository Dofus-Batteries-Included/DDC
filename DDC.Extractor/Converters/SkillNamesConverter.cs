using Core.DataCenter.Metadata.Interactive;
using DDC.Extractor.Models.Skills;

namespace DDC.Extractor.Converters;

public class SkillNamesConverter : IConverter<SkillNames, SkillName>
{
    public SkillName Convert(SkillNames data) => new(data);
}
