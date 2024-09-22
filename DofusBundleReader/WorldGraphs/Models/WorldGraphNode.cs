namespace DofusBundleReader.WorldGraphs.Models;

public class WorldGraphNode
{
    /// <summary>
    ///     The unique ID of the node
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    ///     The ID of the underlying map.
    /// </summary>
    /// <remarks>
    ///     The ID of the map is only unique in a given zone.
    /// </remarks>
    public long MapId { get; set; }

    /// <summary>
    ///     The zone of the underlying map
    /// </summary>
    public int ZoneId { get; set; }
}
