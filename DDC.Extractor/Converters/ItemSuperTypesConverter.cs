using System.Linq;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemSuperTypesConverter : IConverter<ItemSuperTypes, ItemSuperType>
{
    public ItemSuperType Convert(ItemSuperTypes type) =>
        new()
        {
            Id = type.id,
            PossiblePositions = type.possiblePositions.ToArray()
        };
}
