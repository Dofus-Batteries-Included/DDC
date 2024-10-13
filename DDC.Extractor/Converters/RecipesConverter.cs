using Core.DataCenter.Metadata.Job;
using DDC.Extractor.Models.Items;

namespace DDC.Extractor.Converters;

public class RecipesConverter : IConverter<Recipes, Recipe>
{
    public Recipe Convert(Recipes data) => new(data);
}
