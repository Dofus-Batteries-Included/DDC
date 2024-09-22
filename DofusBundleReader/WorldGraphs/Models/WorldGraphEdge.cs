namespace DofusBundleReader.WorldGraphs.Models;

public class WorldGraphEdge
{
    /// <summary>
    ///     The ID of the source node
    /// </summary>
    public long From { get; set; }

    /// <summary>
    ///     The ID of the target node
    /// </summary>
    public long To { get; set; }

    public IReadOnlyCollection<WorldGraphEdgeTransition>? Transitions { get; set; }
}
