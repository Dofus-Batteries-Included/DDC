using Core.DataCenter.Metadata.Interactive;

namespace DDC.Extractor.Models.Worlds;

public class Interactive
{
    public Interactive(Interactives data)
    {
        Id = data.id;
        NameId = data.nameId;
        ActionId = data.actionId;
        DisplayTooltip = data.displayTooltip;
    }

    public int Id { get; set; }
    public int NameId { get; set; }
    public int ActionId { get; set; }
    public bool DisplayTooltip { get; set; }
}
