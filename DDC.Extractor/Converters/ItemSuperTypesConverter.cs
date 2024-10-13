using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemSuperTypesConverter : IConverter<ItemSuperTypes, ItemSuperType>
{
    public ItemSuperType Convert(ItemSuperTypes type) => new(type);
}
