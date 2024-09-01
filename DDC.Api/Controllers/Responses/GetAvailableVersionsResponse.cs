namespace DDC.Api.Controllers.Responses;

/// <summary>
/// </summary>
public class GetAvailableVersionsResponse
{
    /// <summary>
    ///     The latest available version
    /// </summary>
    public required string Latest { get; init; }

    /// <summary>
    ///     The available versions
    /// </summary>
    public required IReadOnlyCollection<string> Versions { get; init; }
}
