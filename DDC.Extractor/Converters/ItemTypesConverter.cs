using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemTypesConverter : IConverter<ItemTypes, ItemType>
{
    public ItemType Convert(ItemTypes type) => new(type);
}
