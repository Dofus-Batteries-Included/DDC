using System.Collections.Generic;
using Core.DataCenter.Metadata.Job;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models.Items;

public class Recipe
{
    public Recipe(Recipes recipe)
    {
        JobId = recipe.jobId;
        ResultId = recipe.resultId;
        ResultNameId = recipe.resultNameId;
        ResultTypeId = recipe.resultTypeId;
        SkillId = recipe.skillId;
        IngredientIds = recipe.ingredientIds.ToCSharpList();
        Quantities = recipe.quantities.ToCSharpList();
        ChangeVersion = recipe.changeVersion;
        TooltipExpirationDate = recipe.tooltipExpirationDate;
    }

    public int JobId { get; set; }
    public int ResultId { get; set; }
    public string ResultNameId { get; set; }
    public uint ResultTypeId { get; set; }
    public int SkillId { get; set; }
    public IReadOnlyList<int> IngredientIds { get; set; }
    public IReadOnlyList<uint> Quantities { get; set; }
    public string ChangeVersion { get; set; }
    public double TooltipExpirationDate { get; set; }
}
