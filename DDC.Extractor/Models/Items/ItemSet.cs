using System.Collections.Generic;
using DDC.Extractor.Models.Effects;

namespace DDC.Extractor.Models.Items;

public class ItemSet
{
    public int Id { get; set; }
    public int NameId { get; set; }
    public IReadOnlyList<uint> Items { get; set; }
    public bool BonusIsSecret { get; set; }
    public IReadOnlyList<IReadOnlyList<EffectInstance>> Effects { get; set; }
}
