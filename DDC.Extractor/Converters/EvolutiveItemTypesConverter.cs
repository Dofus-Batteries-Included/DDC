using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class EvolutiveItemTypesConverter : IConverter<EvolutiveItemTypes, EvolutiveItemType>
{
    public EvolutiveItemType Convert(EvolutiveItemTypes type) =>
        new()
        {
            Id = type.id,
            MaxLevel = type.maxLevel,
            ExperienceBoost = type.experienceBoost,
            ExperienceByLevel = type.experienceByLevel.ToCSharpList()
        };
}
