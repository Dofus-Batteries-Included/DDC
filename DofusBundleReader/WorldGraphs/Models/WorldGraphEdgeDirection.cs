namespace DofusBundleReader.WorldGraphs.Models;

/// <summary>
///     Type of <c>Core.PathFinding.WorldPathfinding.Transition.m_direction</c>
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
