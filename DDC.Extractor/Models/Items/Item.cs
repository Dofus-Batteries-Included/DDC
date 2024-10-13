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
        IconId = item.iconId;
        NameId = item.nameId;
        DescriptionId = item.descriptionId;
        ItemTypeId = item.typeId;
        PossibleEffects = item.possibleEffects.ToCSharpList().Select(e => e.ToInstance()).ToArray();
        EvolutiveEffectIds = item.evolutiveEffectIds.ToCSharpList();
        Price = item.price;
        Weight = item.weight;
        RealWeight = item.realWeight;
        RecyclingNuggets = item.recyclingNuggets;
        FavoriteRecyclingSubAreas = item.favoriteRecyclingSubareas.ToCSharpList();
        ResourcesBySubarea = item.resourcesBySubarea.ToCSharpList().Select(rl => rl.values.ToCSharpList()).ToArray();
        FavoriteSubAreas = item.favoriteSubAreas.ToCSharpList();
        FavoriteSubAreaBonus = item.favoriteSubAreasBonus;
        RecipeIds = item.recipeIds.ToCSharpList();
        RecipeSlots = item.recipeSlots;
        SecretRecipe = item.secretRecipe;
        CraftXpRatio = item.craftXpRatio;
        CraftVisible = item.craftVisible;
        CraftFeasible = item.craftFeasible;
        CraftConditional = item.craftConditional;
        DropMonsterIds = item.dropMonsterIds.ToCSharpList();
        DropTemporisMonsterIds = item.dropMonsterIds.ToCSharpList();
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
        UseAnimationId = item.useAnimationId;
        Visibility = item.visibility;
        Criteria = item.criteria;
        CriteriaTarget = item.criteriaTarget;
        AppearanceId = item.appearanceId;
        ImportantNoticeId = item.importantNoticeId;
        ChangeVersion = item.changeVersion;
        TooltipExpirationDate = item.tooltipExpirationDate;
    }


    public ushort Id { get; set; }
    public byte Level { get; set; }
    public int IconId { get; set; }
    public uint NameId { get; set; }
    public uint DescriptionId { get; set; }
    public int ItemTypeId { get; set; }
    public IReadOnlyList<EffectInstance> PossibleEffects { get; set; }
    public IReadOnlyList<ushort> EvolutiveEffectIds { get; set; }
    public float Price { get; set; }
    public uint Weight { get; set; }
    public uint RealWeight { get; set; }
    public float RecyclingNuggets { get; set; }
    public IReadOnlyList<int> FavoriteRecyclingSubAreas { get; set; }
    public IReadOnlyList<IReadOnlyList<int>> ResourcesBySubarea { get; set; }
    public IReadOnlyList<ushort> FavoriteSubAreas { get; set; }
    public ushort FavoriteSubAreaBonus { get; set; }
    public IReadOnlyList<ushort> RecipeIds { get; set; }
    public byte RecipeSlots { get; set; }
    public bool SecretRecipe { get; set; }
    public short CraftXpRatio { get; set; }
    public string CraftVisible { get; set; }
    public string CraftFeasible { get; set; }
    public string CraftConditional { get; set; }
    public IReadOnlyList<ushort> DropMonsterIds { get; set; }
    public IReadOnlyList<ushort> DropTemporisMonsterIds { get; set; }
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
    public sbyte UseAnimationId { get; set; }
    public string Visibility { get; set; }
    public string Criteria { get; set; }
    public string CriteriaTarget { get; set; }
    public ushort AppearanceId { get; set; }
    public string ImportantNoticeId { get; set; }
    public string ChangeVersion { get; set; }
    public double TooltipExpirationDate { get; set; }
}
