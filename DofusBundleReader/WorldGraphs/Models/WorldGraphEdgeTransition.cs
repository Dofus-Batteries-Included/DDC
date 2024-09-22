namespace DofusBundleReader.WorldGraphs.Models;

public class WorldGraphEdgeTransition
{
    /// <summary>
    ///     The type of transition
    /// </summary>
    public WorldGraphEdgeType? Type { get; set; }

    /// <summary>
    ///     The direction of the transition
    /// </summary>
    public WorldGraphEdgeDirection? Direction { get; set; }

    /// <summary>
    ///     The ID of the map
    /// </summary>
    public long MapId { get; set; }
}
