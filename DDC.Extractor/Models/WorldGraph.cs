using System;
using System.Collections.Generic;

namespace DDC.Extractor.Models;

public class WorldGraph
{
    public IReadOnlyCollection<WorldGraphNode> Nodes { get; set; }
    public IReadOnlyCollection<WorldGraphEdge> Edges { get; set; }
}

public class WorldGraphNode
{
    /// <summary>
    ///     The unique ID of the node
    /// </summary>
    public int Id { get; set; }

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

public class WorldGraphEdge
{
    /// <summary>
    ///     The ID of the source node
    /// </summary>
    public int From { get; set; }

    /// <summary>
    ///     The ID of the target node
    /// </summary>
    public int To { get; set; }

    /// <summary>
    ///     The type of transition
    /// </summary>
    public IReadOnlyCollection<WorldGraphEdgeType> Types { get; set; }

    /// <summary>
    ///     The direction of the transition
    /// </summary>
    public WorldGraphEdgeDirection Direction { get; set; }
}

/// <summary>
///     Type of <see cref="Core.PathFinding.WorldPathfinding.Transition.m_direction" />
/// </summary>
public enum WorldGraphEdgeDirection
{
    Random = -4, // 0xFFFFFFFC
    Same = -3, // 0xFFFFFFFD
    Opposite = -2, // 0xFFFFFFFE
    Invalid = -1, // 0xFFFFFFFF
    East = 0,
    SouthEast = 1,
    South = 2,
    SouthWest = 3,
    West = 4,
    NorthWest = 5,
    North = 6,
    NorthEast = 7
}

public enum WorldGraphEdgeType
{
    Unspecified = 0,
    Scroll = 1,
    ScrollAction = 2,
    MapEvent = 3,
    MapAction = 4,
    MapObstacle = 5,
    Interactive = 6,
    NpcAction = 7
}