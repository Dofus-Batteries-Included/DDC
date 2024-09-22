using System.Collections.Generic;
using DDC.Extractor.Common.Models;

namespace DDC.Extractor.Areas.Models;

public class SubArea
{
    public int Id { get; set; }
    public int NameId { get; set; }
    public int AreaId { get; set; }
    public int WorldMapId { get; set; }
    public uint Level { get; set; }
    public Position Center { get; set; }
    public Bounds Bounds { get; set; }
    public IReadOnlyCollection<int> Neighbours { get; set; }
    public IReadOnlyCollection<int> EntranceMapIds { get; set; }
    public IReadOnlyCollection<int> ExitMapIds { get; set; }
    public int? ZaapMapId { get; set; }
    public bool Capturable { get; set; }
    public bool BasicAccountAllowed { get; set; }
    public bool PsiAllowed { get; set; }
    public bool MountAutoTripAllowed { get; set; }
    public bool IsConquestVillage { get; set; }
    public bool DisplayOnWorldMap { get; set; }
    public IReadOnlyCollection<uint> CustomWorldMap { get; set; }
}
