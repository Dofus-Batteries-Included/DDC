using System.IO.Compression;
using System.Text.Json;
using DDC.Api.Exceptions;
using DDC.Api.Workers;
using Microsoft.Extensions.Options;

namespace DDC.Api.Repositories;

class RawDataFromGithubReleasesSavedToDisk : IRawDataRepository
{
    readonly IOptions<RepositoryOptions> _repositoryOptions;
    readonly ILogger<RawDataFromGithubReleasesSavedToDisk> _logger;
    readonly JsonSerializerOptions _ddcMetadataJsonSerializerOptions = new() { WriteIndented = true, PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower };

    public RawDataFromGithubReleasesSavedToDisk(IOptions<RepositoryOptions> repositoryOptions, ILogger<RawDataFromGithubReleasesSavedToDisk> logger)
    {
        _repositoryOptions = repositoryOptions;
        _logger = logger;
    }

    public Task<string> GetLatestVersionAsync() =>
        Task.FromResult(GetActualVersions().OrderDescending().FirstOrDefault() ?? throw new NotFoundException("Could not find any version."));

    public Task<IReadOnlyCollection<string>> GetAvailableVersionsAsync() => Task.FromResult<IReadOnlyCollection<string>>(GetActualVersions().ToList());

    public Task<IRawDataFile> GetRawDataFileAsync(string version, RawDataType type, CancellationToken cancellationToken = default)
    {
        string? versionDirectory = GetVersionDirectory(version);
        if (versionDirectory == null)
        {
            throw new NotFoundException($"Could not find data for version {version}.");
        }

        string versionPath = Path.Join(_repositoryOptions.Value.RawDataPath, versionDirectory);
        if (!Directory.Exists(versionPath))
        {
            throw new NotFoundException($"Could not find data for version {version}.");
        }

        string path = Path.Join(versionPath, GetFilename(type));
        if (!Path.Exists(path))
        {
            throw new NotFoundException($"Could not find data for for type {type} in version {version}.");
        }

        return Task.FromResult<IRawDataFile>(new File(path));
    }

    public async Task<SavedDataSummary> GetSavedDataSummaryAsync(CancellationToken cancellationToken = default)
    {
        List<string> versions = GetActualVersions().ToList();
        Dictionary<string, DdcMetadata> versionsMetadata = new();
        foreach (string version in versions)
        {
            string path = Path.Join(_repositoryOptions.Value.RawDataPath, version);
            DdcMetadata? metadata = await ReadDdcMetadataAsync(path, cancellationToken);
            if (metadata == null)
            {
                _logger.LogWarning("Could not find DDC metadata at {Path}.", path);
                continue;
            }

            versionsMetadata[version] = metadata;
        }

        return new SavedDataSummary(versions, versionsMetadata);
    }

    public async Task SaveRawDataFilesAsync(DownloadDataFromGithubReleases.Release release, string gameVersion, ZipArchive archive, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Saving data from release {Name} containing version {Version}.", release.Name, gameVersion);

        string path = Path.Join(_repositoryOptions.Value.RawDataPath, gameVersion);
        Directory.CreateDirectory(path);

        await WriteDdcMetadataAsync(release, path, cancellationToken);

        foreach (ZipArchiveEntry entry in archive.Entries)
        {
            string entryFullPath = Path.Join(path, entry.FullName);
            string? entryDirectory = Path.GetDirectoryName(entryFullPath);
            if (entryDirectory != null && !Directory.Exists(entryDirectory))
            {
                Directory.CreateDirectory(entryFullPath);
            }

            _logger.LogDebug("Writing file {Path}...", entryFullPath);

            await using FileStream file = System.IO.File.OpenWrite(entryFullPath);
            await using Stream entryStream = entry.Open();
            await entryStream.CopyToAsync(file, cancellationToken);
        }
    }

    string? GetVersionDirectory(string version) =>
        version switch
        {
            "latest" => GetActualVersions().OrderDescending().FirstOrDefault(),
            _ => version
        };

    static string GetFilename(RawDataType type) =>
        type switch
        {
            RawDataType.I18NFr => "fr.i18n.json",
            RawDataType.I18NEn => "en.i18n.json",
            RawDataType.I18NEs => "es.i18n.json",
            RawDataType.I18NDe => "de.i18n.json",
            RawDataType.I18NPt => "pt.i18n.json",
            RawDataType.MapPositions => "map-positions.json",
            RawDataType.PointOfInterest => "point-of-interest.json",
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null)
        };

    IEnumerable<string> GetActualVersions() =>
        Directory.Exists(_repositoryOptions.Value.RawDataPath)
            ? Directory.EnumerateDirectories(_repositoryOptions.Value.RawDataPath).Select(Path.GetFileName).OfType<string>()
            : [];

    async Task WriteDdcMetadataAsync(DownloadDataFromGithubReleases.Release release, string directory, CancellationToken cancellationToken)
    {
        DdcMetadata ddcMetadata = new() { ReleaseUrl = release.HtmlUrl, ReleaseName = release.Name };
        string ddcMetadataPath = Path.Join(directory, "ddc-metadata.json");
        await using FileStream ddcMetadataStream = System.IO.File.OpenWrite(ddcMetadataPath);
        await JsonSerializer.SerializeAsync(ddcMetadataStream, ddcMetadata, _ddcMetadataJsonSerializerOptions, cancellationToken);
    }

    async Task<DdcMetadata?> ReadDdcMetadataAsync(string directory, CancellationToken cancellationToken)
    {
        string ddcMetadataPath = Path.Join(directory, "ddc-metadata.json");
        if (!System.IO.File.Exists(ddcMetadataPath))
        {
            return null;
        }

        await using FileStream ddcMetadataStream = System.IO.File.OpenRead(ddcMetadataPath);
        return await JsonSerializer.DeserializeAsync<DdcMetadata>(ddcMetadataStream, _ddcMetadataJsonSerializerOptions, cancellationToken);
    }

    class File : IRawDataFile
    {
        readonly string _filepath;

        public File(string filepath)
        {
            _filepath = filepath;
            Name = Path.GetFileName(filepath);
        }

        public string Name { get; }
        public string ContentType { get; } = "application/json";
        public Stream OpenRead() => System.IO.File.OpenRead(_filepath);
    }

    public class SavedDataSummary
    {
        readonly IReadOnlyList<string> _versions;
        readonly Dictionary<string, DdcMetadata> _metadata;

        public SavedDataSummary(IReadOnlyList<string> versions, Dictionary<string, DdcMetadata> metadata)
        {
            _versions = versions;
            _metadata = metadata;
        }

        public IReadOnlyList<string> GetVersions() => _versions;
        public DdcMetadata? GetMetadata(string version) => _metadata.GetValueOrDefault(version);
    }

    public class DdcMetadata
    {
        public required string ReleaseUrl { get; init; }
        public required string ReleaseName { get; init; }
    }
}
