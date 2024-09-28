using DDC.Extractor.Common.Models;

namespace DDC.Extractor.Areas.Models;

public class WorldMap
{
    public int Id { get; set; }
    public int NameId { get; set; }
    public Position Origin { get; set; }
}
