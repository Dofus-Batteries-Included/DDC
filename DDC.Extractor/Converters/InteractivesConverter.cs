using Core.DataCenter.Metadata.Interactive;
using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class InteractivesConverter : IConverter<Interactives, Interactive>
{
    public Interactive Convert(Interactives data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            ActionId = data.actionId,
            DisplayTooltip = data.displayTooltip
        };
}
