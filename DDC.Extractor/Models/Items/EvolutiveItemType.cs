using System.Collections.Generic;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Items;

public class EvolutiveItemType
{
    public EvolutiveItemType(EvolutiveItemTypes type)
    {
        Id = type.id;
        MaxLevel = type.maxLevel;
        ExperienceBoost = type.experienceBoost;
        ExperienceByLevel = type.experienceByLevel.ToCSharpList();
    }

    public int Id { get; set; }
    public int MaxLevel { get; set; }
    public double ExperienceBoost { get; set; }
    public IReadOnlyList<int> ExperienceByLevel { get; set; }
}
