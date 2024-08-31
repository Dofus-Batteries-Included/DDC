using DDC.Api.Exceptions;

namespace DDC.Api.Repositories;

class JsonRawDataFilesOnDisk : IRawDataRepository
{
    readonly string _directory;
    static readonly string[] DefaultVersions = ["latest"];

    public JsonRawDataFilesOnDisk(string directory)
    {
        _directory = directory;
    }

    public Task<IReadOnlyCollection<string>> GetAvailableVersionsAsync()
    {
        IEnumerable<string> versionDirs = GetActualVersions();
        return Task.FromResult<IReadOnlyCollection<string>>(DefaultVersions.Concat(versionDirs).ToArray());
    }

    public Task<IRawDataFile> GetRawDataFileAsync(string version, RawDataType type, CancellationToken cancellationToken = default)
    {
        string? versionDirectory = GetVersionDirectory(version);
        if (versionDirectory == null)
        {
            throw new BadRequestException($"Could not find data for version {version}.");
        }

        string versionPath = Path.Join(_directory, versionDirectory);
        if (!Directory.Exists(versionPath))
        {
            throw new BadRequestException($"Could not find data for version {version}.");
        }

        string path = Path.Join(versionPath, GetFilename(type));
        if (!Path.Exists(path))
        {
            throw new BadRequestException($"Could not find data for for type {type} in version {version}.");
        }

        return Task.FromResult<IRawDataFile>(new File(path));
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

    IEnumerable<string> GetActualVersions() => Directory.EnumerateDirectories(_directory).Select(Path.GetFileName).OfType<string>();

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
}
