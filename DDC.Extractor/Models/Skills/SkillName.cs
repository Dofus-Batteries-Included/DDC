using Core.DataCenter.Metadata.Interactive;

namespace DDC.Extractor.Models.Skills;

public class SkillName
{
    public SkillName(SkillNames names)
    {
        Id = names.id;
        NameId = names.nameId;
    }

    public int Id { get; set; }
    public int NameId { get; set; }
}
