namespace DDC.Api.Repositories;

/// <summary>
/// </summary>
public class RepositoryOptions
{
    /// <summary>
    ///     Base path of repository
    /// </summary>
    public string BasePath { get; set; } = Path.GetFullPath("AppData");

    /// <summary>
    ///     Path of the Raw data directory
    /// </summary>
    public string RawDataPath => Path.Join(BasePath, "Raw");
}
