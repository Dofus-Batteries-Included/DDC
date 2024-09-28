using DDC.Extractor.Models;

namespace DDC.Extractor.Converters;

public class AreasConverter : IConverter<Core.DataCenter.Metadata.World.Areas, Area>
{
    public Area Convert(Core.DataCenter.Metadata.World.Areas data) =>
        new()
        {
            Id = data.id,
            NameId = data.nameId,
            WorldMapId = data.hasWorldMap ? data.worldmapId : null,
            SuperAreaId = data.superAreaId,
            ContainHouses = data.containHouses,
            ContainPaddocks = data.containPaddocks,
            Bounds = new Bounds
            {
                Position = new Position
                {
                    X = (int)data.bounds.x,
                    Y = (int)data.bounds.y
                },
                Size = new Size
                {
                    Width = (int)data.bounds.width,
                    Height = (int)data.bounds.height
                }
            }
        };
}
