using System.Collections.Generic;
using System.Linq;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Effects;

namespace DDC.Extractor.Models.Items;

public class ItemSet
{
    public ItemSet(ItemSets set)
    {
        Id = set.id;
        NameId = set.nameId;
        Items = set.items.ToCSharpList();
        Effects = set.effects.ToCSharpList().Where(el => el != null).Select(el => el.values.ToCSharpList().Where(e => e != null).Select(e => e.ToDdc()).ToArray()).ToArray();
        BonusIsSecret = set.bonusIsSecret;
    }

    public int Id { get; set; }
    public int NameId { get; set; }
    public IReadOnlyList<uint> Items { get; set; }
    public bool BonusIsSecret { get; set; }
    public IReadOnlyList<IReadOnlyList<EffectInstance>> Effects { get; set; }
}
