using System.Collections.Generic;
using System.Linq;
using Core.DataCenter.Metadata.World;
using Core.DataCenter.Types;
using DDC.Extractor.Abstractions;
using DDC.Extractor.Areas.Models;
using DDC.Extractor.Common.Models;

namespace DDC.Extractor.Areas;

public class SubAreasConverter : IConverter<SubAreas, SubArea>
{
    public SubArea Convert(SubAreas data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            AreaId = data.areaId,
            WorldMapId = data.worldmapId,
            Level = data.level,
            Center = NonEmptyPositionOrNull(data.m_center),
            Bounds = NonEmptyBoundsOrNull(data.bounds),
            Neighbours = NonEmptyCollectionOrNull(data.neighbors),
            EntranceMapIds = NonEmptyCollectionOrNull(data.entranceMapIds),
            ExitMapIds = NonEmptyCollectionOrNull(data.exitMapIds),
            ZaapMapId = data.associatedZaapMapId == 0 ? null : data.associatedZaapMapId,
            Capturable = data.capturable,
            BasicAccountAllowed = data.basicAccountAllowed,
            PsiAllowed = data.psiAllowed,
            MountAutoTripAllowed = data.mountAutoTripAllowed,
            IsConquestVillage = data.isConquestVillage,
            DisplayOnWorldMap = data.displayOnWorldMap,
            CustomWorldMap = data.hasCustomWorldMap ? NonEmptyCollectionOrNull(data.customWorldMap) : null
        };

    static IReadOnlyCollection<T> NonEmptyCollectionOrNull<T>(Il2CppSystem.Collections.Generic.List<T> data) => data.Count == 0 ? null : new List<T>(data._items.Take(data.Count));

    static Position NonEmptyPositionOrNull(Point position)
    {
        if (position.x == 0 && position.y == 0)
        {
            return null;
        }

        return new Position
        {
            X = position.x,
            Y = position.y
        };
    }

    static Bounds NonEmptyBoundsOrNull(Rectangle bounds)
    {
        if (bounds.width == 0 && bounds.height == 0)
        {
            return null;
        }

        return new Bounds
        {
            Position = new Position
            {
                X = (int)bounds.x,
                Y = (int)bounds.y
            },
            Size = new Size
            {
                Width = (int)bounds.width,
                Height = (int)bounds.height
            }
        };
    }
}
