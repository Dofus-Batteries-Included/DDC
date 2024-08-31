namespace DDC.Api.Repositories;

/// <summary>
///     Raw data files repository.
///     This abstraction allows to retrieve files from any kind of storage, e.g. the local file system, or a remote object storage.
/// </summary>
public interface IRawDataRepository
{
    /// <summary>
    ///     Get a file containing the requested data for the requested version of the game.
    /// </summary>
    Task<IRawDataFile> GetRawDataFileAsync(string version, RawDataType type, CancellationToken cancellationToken = default);
}
