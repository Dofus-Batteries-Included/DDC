using System.Collections.Generic;
using Core.DataCenter.Metadata.World;
using DDC.Extractor.Extensions;

namespace DDC.Extractor.Models;

public class SubArea
{
    public SubArea(SubAreas data)
    {
        Id = data.id;
        NameId = data.nameId;
        AreaId = data.areaId;
        WorldMapId = data.worldmapId;
        Level = data.level;
        Center = data.m_center.ToNonEmptyPositionOrNull();
        Bounds = data.bounds.ToNonEmptyBoundsOrNull();
        Neighbours = data.neighbors.ToNonEmptyCSharpListOrNull();
        EntranceMapIds = data.entranceMapIds.ToNonEmptyCSharpListOrNull();
        ExitMapIds = data.exitMapIds.ToNonEmptyCSharpListOrNull();
        ZaapMapId = data.associatedZaapMapId == 0 ? null : data.associatedZaapMapId;
        Capturable = data.capturable;
        BasicAccountAllowed = data.basicAccountAllowed;
        PsiAllowed = data.psiAllowed;
        MountAutoTripAllowed = data.mountAutoTripAllowed;
        IsConquestVillage = data.isConquestVillage;
        DisplayOnWorldMap = data.displayOnWorldMap;
        CustomWorldMap = data.hasCustomWorldMap ? data.customWorldMap.ToNonEmptyCSharpListOrNull() : null;
    }

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
