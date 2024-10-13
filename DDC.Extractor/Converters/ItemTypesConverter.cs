using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemTypesConverter : IConverter<ItemTypes, ItemType>
{
    public ItemType Convert(ItemTypes type) =>
        new()
        {
            Id = type.id,
            NameId = type.nameId,
            Category = type.categoryId,
            Gender = type.gender,
            Plural = type.plural,
            SuperTypeId = type.superTypeId,
            EvolutiveTypeId = type.evolutiveTypeId,
            Mimickable = type.mimickable,
            CraftXpRatio = type.craftXpRatio,
            PossiblePositions = type.possiblePositions.ToCSharpList(),
            RawZone = type.rawZone,
            IsInEncyclopedia = type.isInEncyclopedia,
            AdminSelectionTypeName = type.adminSelectionTypeName
        };
}
