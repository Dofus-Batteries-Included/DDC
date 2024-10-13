using System.Linq;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Effects;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemSetsConverter : IConverter<ItemSets, ItemSet>
{
    public ItemSet Convert(ItemSets set) =>
        new()
        {
            Id = set.id,
            NameId = set.nameId,
            Items = set.items.ToCSharpList(),
            Effects = set.effects.ToCSharpList()
                .Where(el => el != null)
                .Select(el => el.values.ToCSharpList().Where(e => e != null).Select(e => e.ToInstance()).ToArray())
                .ToArray(),
            BonusIsSecret = set.bonusIsSecret
        };
}
