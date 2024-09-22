using MessagePack;

namespace DofusBundleReader.WorldGraphs.Models;

[MessagePackObject]
public class WorldGraphEdgeTransition
{
    /// <summary>
    ///     The type of transition
    /// </summary>
    [Key(0)]
    public WorldGraphEdgeType? Type { get; set; }

    /// <summary>
    ///     The direction of the transition
    /// </summary>
    [Key(1)]
    public WorldGraphEdgeDirection? Direction { get; set; }

    /// <summary>
    ///     The ID of the map
    /// </summary>
    [Key(2)]
    public long MapId { get; set; }
}
