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

    /// <summary>
    ///     The ID of the skill used to take the transition
    /// </summary>
    public int SkillId { get; set; }

    /// <summary>
    ///     The ID of the cell from which the user can take the transition
    /// </summary>
    public int CellId { get; set; }

    /// <summary>
    ///     The condition that the user must fulfill to be able to take the transition
    /// </summary>
    public string? Criterion { get; set; }
}
