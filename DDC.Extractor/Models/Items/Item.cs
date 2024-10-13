using System.Collections.Generic;
using System.Linq;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Effects;

namespace DDC.Extractor.Models.Items;

public class Item
{
    public Item(Core.DataCenter.Metadata.Item.Items item)
    {
        Id = item.id;
        Level = item.level;
        NameId = item.nameId;
        DescriptionId = item.descriptionId;
        ItemTypeId = item.typeId;
        PossibleEffects = item.possibleEffects.ToCSharpList().Select(e => e.ToInstance()).ToArray();
        Price = item.price;
        Weight = item.weight;
        RecyclingNuggets = item.recyclingNuggets;
        RecipeIds = item.recipeIds.ToCSharpList();
        RecipeSlots = item.recipeSlots;
        SecretRecipe = item.secretRecipe;
        ItemSetId = item.itemSetId;
        TwoHanded = item.twoHanded;
        Usable = item.usable;
        NeedUseConfirm = item.needUseConfirm;
        NonUsableOnAnother = item.nonUsableOnAnother;
        Targetable = item.targetable;
        Exchangeable = item.exchangeable;
        Enhanceable = item.enhanceable;
        Ethereal = item.etheral;
        Cursed = item.cursed;
        IsDestructible = item.isDestructible;
        IsLegendary = item.isLegendary;
        IsColorable = item.isColorable;
        IsSealable = item.isSaleable;
        HideEffects = item.hideEffects;
        BonusIsSecret = item.bonusIsSecret;
        ObjectIsDisplayOnWeb = item.objectIsDisplayOnWeb;
    }

    public ushort Id { get; set; }
    public byte Level { get; set; }
    public uint NameId { get; set; }
    public uint DescriptionId { get; set; }
    public int ItemTypeId { get; set; }
    public IReadOnlyList<EffectInstance> PossibleEffects { get; set; }
    public float Price { get; set; }
    public uint Weight { get; set; }
    public float RecyclingNuggets { get; set; }
    public IReadOnlyList<ushort> RecipeIds { get; set; }
    public byte RecipeSlots { get; set; }
    public bool SecretRecipe { get; set; }
    public short ItemSetId { get; set; }
    public bool TwoHanded { get; set; }
    public bool Usable { get; set; }
    public bool NeedUseConfirm { get; set; }
    public bool NonUsableOnAnother { get; set; }
    public bool Targetable { get; set; }
    public bool Exchangeable { get; set; }
    public bool Enhanceable { get; set; }
    public bool Ethereal { get; set; }
    public bool Cursed { get; set; }
    public bool IsDestructible { get; set; }
    public bool IsLegendary { get; set; }
    public bool IsColorable { get; set; }
    public bool IsSealable { get; set; }
    public bool HideEffects { get; set; }
    public bool BonusIsSecret { get; set; }
    public bool ObjectIsDisplayOnWeb { get; set; }
}
