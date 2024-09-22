using MessagePack;

namespace DofusBundleReader.WorldGraphs.Models;

[MessagePackObject]
public class WorldGraphNode
{
    /// <summary>
    ///     The unique ID of the node
    /// </summary>
    [Key(0)]
    public long Id { get; set; }

    /// <summary>
    ///     The ID of the underlying map.
    /// </summary>
    /// <remarks>
    ///     The ID of the map is only unique in a given zone.
    /// </remarks>
    [Key(1)]
    public long MapId { get; set; }

    /// <summary>
    ///     The zone of the underlying map
    /// </summary>
    [Key(2)]
    public int ZoneId { get; set; }
}
