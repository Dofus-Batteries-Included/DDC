using MessagePack;

namespace DofusBundleReader.WorldGraphs.Models;

[MessagePackObject]
public class WorldGraph
{
    [Key(0)]
    public IReadOnlyCollection<WorldGraphNode> Nodes { get; set; } = [];

    [Key(1)]
    public IReadOnlyCollection<WorldGraphEdge> Edges { get; set; } = [];
}
