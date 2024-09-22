using MessagePack;

namespace DofusBundleReader.Maps.Models;

[MessagePackObject]
public class Map
{
    [Key(0)]
    public required Dictionary<int, Cell> Cells { get; init; }
}
