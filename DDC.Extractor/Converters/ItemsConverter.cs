using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemsConverter : IConverter<Items, Item>
{
    public Item Convert(Items item) => new(item);
}
