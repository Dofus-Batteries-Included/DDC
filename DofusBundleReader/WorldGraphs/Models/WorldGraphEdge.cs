using MessagePack;

namespace DofusBundleReader.WorldGraphs.Models;

[MessagePackObject]
public class WorldGraphEdge
{
    /// <summary>
    ///     The ID of the source node
    /// </summary>
    [Key(0)]
    public long From { get; set; }

    /// <summary>
    ///     The ID of the target node
    /// </summary>
    [Key(1)]
    public long To { get; set; }

    [Key(2)]
    public IReadOnlyCollection<WorldGraphEdgeTransition>? Transitions { get; set; }
}
