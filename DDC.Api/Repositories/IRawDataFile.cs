namespace DDC.Api.Repositories;

/// <summary>
///     Raw data file
/// </summary>
public interface IRawDataFile
{
    /// <summary>
    ///     The name of the file
    /// </summary>
    string Name { get; }

    /// <summary>
    ///     The content type of the data
    /// </summary>
    string ContentType { get; }

    /// <summary>
    ///     Get a stream to read data from the file
    /// </summary>
    Stream OpenRead();
}
