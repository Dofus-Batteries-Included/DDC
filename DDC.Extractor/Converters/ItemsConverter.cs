using System.Linq;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Effects;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemsConverter : IConverter<Items, Item>
{
    public Item Convert(Items item) =>
        new()
        {
            Id = item.id,
            Level = item.level,
            NameId = item.nameId,
            DescriptionId = item.descriptionId,
            ItemTypeId = item.typeId,
            PossibleEffects = item.possibleEffects.ToCSharpList().Select(e => e.ToInstance()).ToArray(),
            Price = item.price,
            Weight = item.weight,
            RecyclingNuggets = item.recyclingNuggets,
            RecipeIds = item.recipeIds.ToCSharpList(),
            RecipeSlots = item.recipeSlots,
            SecretRecipe = item.secretRecipe,
            ItemSetId = item.itemSetId,
            TwoHanded = item.twoHanded,
            Usable = item.usable,
            NeedUseConfirm = item.needUseConfirm,
            NonUsableOnAnother = item.nonUsableOnAnother,
            Targetable = item.targetable,
            Exchangeable = item.exchangeable,
            Enhanceable = item.enhanceable,
            Ethereal = item.etheral,
            Cursed = item.cursed,
            IsDestructible = item.isDestructible,
            IsLegendary = item.isLegendary,
            IsColorable = item.isColorable,
            IsSealable = item.isSaleable,
            HideEffects = item.hideEffects,
            BonusIsSecret = item.bonusIsSecret,
            ObjectIsDisplayOnWeb = item.objectIsDisplayOnWeb
        };
}
