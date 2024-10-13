using System.Linq;
using Core.DataCenter.Metadata.Item;
using DDC.Extractor.Extensions;
using DDC.Extractor.Models.Effects;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class ItemsConverter : IConverter<Items, Item>
{
    public Item Convert(Items data) =>
        new()
        {
            Id = data.id,
            Level = data.level,
            NameId = data.nameId,
            DescriptionId = data.descriptionId,
            Category = data.category,
            PossibleEffects = data.possibleEffects.ToCSharpList().Select(e => e.ToInstance()).ToArray(),
            Price = data.price,
            Weight = data.weight,
            RecyclingNuggets = data.recyclingNuggets,
            ItemTypeId = data.itemType.id,
            RecipeIds = data.recipeIds.ToCSharpList(),
            RecipeSlots = data.recipeSlots,
            SecretRecipe = data.secretRecipe,
            ItemSetId = data.itemSetId,
            TwoHanded = data.twoHanded,
            Usable = data.usable,
            NeedUseConfirm = data.needUseConfirm,
            NonUsableOnAnother = data.nonUsableOnAnother,
            Targetable = data.targetable,
            Exchangeable = data.exchangeable,
            Enhanceable = data.enhanceable,
            Ethereal = data.etheral,
            Cursed = data.cursed,
            IsDestructible = data.isDestructible,
            IsLegendary = data.isLegendary,
            IsColorable = data.isColorable,
            IsSealable = data.isSaleable,
            HideEffects = data.hideEffects,
            BonusIsSecret = data.bonusIsSecret,
            ObjectIsDisplayOnWeb = data.objectIsDisplayOnWeb
        };
}
