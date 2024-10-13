using System.Collections.Generic;

namespace DDC.Extractor.Models.Items;

public class ItemType
{
    public int Id { get; set; }
    public int NameId { get; set; }
    public Core.DataCenter.Metadata.Item.Items.ItemCategoryEnum Category { get; set; }
    public int Gender { get; set; }
    public bool Plural { get; set; }
    public int SuperTypeId { get; set; }
    public int EvolutiveTypeId { get; set; }
    public bool Mimickable { get; set; }
    public int CraftXpRatio { get; set; }
    public IReadOnlyList<int> PossiblePositions { get; set; }
    public string RawZone { get; set; }
    public bool IsInEncyclopedia { get; set; }
    public string AdminSelectionTypeName { get; set; }
}
