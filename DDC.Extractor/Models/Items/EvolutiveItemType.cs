using System.Collections.Generic;

namespace DDC.Extractor.Models.Items;

public class EvolutiveItemType
{
    public int Id { get; set; }
    public int MaxLevel { get; set; }
    public double ExperienceBoost { get; set; }
    public IReadOnlyList<int> ExperienceByLevel { get; set; }
}
