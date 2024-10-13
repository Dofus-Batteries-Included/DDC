using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemSetsConverter : IConverter<ItemSets, ItemSet>
{
    public ItemSet Convert(ItemSets set) => new(set);
}
