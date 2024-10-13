using System.Collections.Generic;
using System.Linq;
using Core.DataCenter.Metadata.Item;

namespace DDC.Extractor.Models.Items;

public class ItemSuperType
{
    public ItemSuperType(ItemSuperTypes type)
    {
        Id = type.id;
        PossiblePositions = type.possiblePositions.ToArray();
    }

    public int Id { get; set; }
    public IReadOnlyList<int> PossiblePositions { get; set; }
}
