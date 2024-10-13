using System.Collections.Generic;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Items;

public class ItemType
{
    public ItemType(ItemTypes type)
    {
        Id = type.id;
        NameId = type.nameId;
        Category = type.categoryId;
        Gender = type.gender;
        Plural = type.plural;
        SuperTypeId = type.superTypeId;
        EvolutiveTypeId = type.evolutiveTypeId;
        Mimickable = type.mimickable;
        CraftXpRatio = type.craftXpRatio;
        PossiblePositions = type.possiblePositions.ToCSharpList();
        RawZone = type.rawZone;
        IsInEncyclopedia = type.isInEncyclopedia;
        AdminSelectionTypeName = type.adminSelectionTypeName;
    }

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
