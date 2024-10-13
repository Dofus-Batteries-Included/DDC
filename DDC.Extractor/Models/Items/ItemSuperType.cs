using System.Collections.Generic;

namespace DDC.Extractor.Models.Items;

public class ItemSuperType
{
    public int Id { get; set; }
    public IReadOnlyList<int> PossiblePositions { get; set; }
}
