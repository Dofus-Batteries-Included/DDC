using System.IO.Compression;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.RegularExpressions;
using DDC.Api.Repositories;

namespace DDC.Api.Workers;

partial class DownloadDataFromGithubReleases : PeriodicService
{
    readonly IHttpClientFactory _httpClientFactory;
    readonly RawDataFromGithubReleasesSavedToDisk _rawDataFromGithubReleasesSavedToDisk;
    readonly HashSet<string> _processedReleases = [];

    public DownloadDataFromGithubReleases(
        IHttpClientFactory httpClientFactory,
        RawDataFromGithubReleasesSavedToDisk rawDataFromGithubReleasesSavedToDisk,
        ILogger<PeriodicService> logger
    ) : base(TimeSpan.FromHours(1), logger)
    {
        _httpClientFactory = httpClientFactory;
        _rawDataFromGithubReleasesSavedToDisk = rawDataFromGithubReleasesSavedToDisk;
    }

    protected override async Task OnTickAsync(CancellationToken stoppingToken)
    {
        Logger.LogInformation("Start refreshing data from DDC Releases.");

        RawDataFromGithubReleasesSavedToDisk.SavedDataSummary versionsMetadata = await _rawDataFromGithubReleasesSavedToDisk.GetSavedDataSummaryAsync(stoppingToken);

        await foreach (Release release in GetReleasesAsync(stoppingToken))
        {
            if (_processedReleases.Contains(release.Name))
            {
                Logger.LogDebug("Skipping release {Name} because it has already been processed.", release.Name);
                continue;
            }

            Stream? dataFile = await DownloadReleaseDataAsync(release, stoppingToken);
            if (dataFile == null)
            {
                Logger.LogWarning("Could not get data from release {Name}.", release.Name);
                continue;
            }

            await using Stream _ = dataFile;
            using ZipArchive zip = new(dataFile, ZipArchiveMode.Read);

            Metadata? metadata = await ReadMetadataAsync(zip, stoppingToken);
            if (metadata == null)
            {
                Logger.LogWarning($"Could not get metadata in data from release {release.Name}.");
                continue;
            }

            RawDataFromGithubReleasesSavedToDisk.DdcMetadata? ddcMetadata = versionsMetadata.GetMetadata(metadata.GameVersion);
            if (ddcMetadata != null && string.Compare(release.Name, ddcMetadata.ReleaseName, StringComparison.InvariantCultureIgnoreCase) <= 0)
            {
                Logger.LogInformation(
                    "Release {ReleaseName} containing data for version {Version} will be ignored because the current data has been extracted from more recent release {MetadataReleaseName}.",
                    release.Name,
                    metadata.GameVersion,
                    ddcMetadata.ReleaseName
                );
            }
            else
            {
                await _rawDataFromGithubReleasesSavedToDisk.SaveRawDataFilesAsync(release, metadata.GameVersion, zip, stoppingToken);
            }

            _processedReleases.Add(release.Name);
        }
    }

    async IAsyncEnumerable<Release> GetReleasesAsync([EnumeratorCancellation] CancellationToken stoppingToken)
    {
        using HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");
        httpClient.DefaultRequestHeaders.Add("User-Agent", "DDC-Api");
        httpClient.DefaultRequestHeaders.Add("X-Github-Api-Version", "2022-11-28");

        string uri = "https://api.github.com/repos/Dofus-Batteries-Included/DDC/releases";
        while (true)
        {
            Logger.LogInformation("Will download releases from: {Uri}", uri);

            HttpResponseMessage httpResponse = await httpClient.GetAsync(uri, stoppingToken);
            httpResponse.EnsureSuccessStatusCode();

            Release[]? responses = await httpResponse.Content.ReadFromJsonAsync<Release[]>(
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower },
                stoppingToken
            );
            if (responses == null)
            {
                Logger.LogError("Could not parse response from Github Releases.");
                break;
            }

            foreach (Release response in responses)
            {
                yield return response;
            }

            if (!httpResponse.Headers.TryGetValues("Link", out IEnumerable<string>? links))
            {
                break;
            }

            Regex nextLinkRegex = NextLinkRegex();
            Match? match = links.Select(l => nextLinkRegex.Match(l)).FirstOrDefault(m => m.Success);
            if (match == null)
            {
                break;
            }

            uri = match.Groups["uri"].Value;
        }
    }

    async Task<Stream?> DownloadReleaseDataAsync(Release release, CancellationToken stoppingToken)
    {
        Asset? dataAsset = release.Assets.FirstOrDefault(a => a.Name == "data.zip");
        if (dataAsset == null)
        {
            return null;
        }

        using HttpClient httpClient = _httpClientFactory.CreateClient();
        httpClient.DefaultRequestHeaders.Add("User-Agent", "DDC-Api");
        HttpResponseMessage response = await httpClient.GetAsync(dataAsset.BrowserDownloadUrl, stoppingToken);
        return await response.Content.ReadAsStreamAsync(stoppingToken);
    }

    static async Task<Metadata?> ReadMetadataAsync(ZipArchive zip, CancellationToken stoppingToken)
    {
        ZipArchiveEntry? metadataEntry = zip.GetEntry("metadata.json");
        if (metadataEntry == null)
        {
            return null;
        }

        await using Stream metadataStream = metadataEntry.Open();
        return await JsonSerializer.DeserializeAsync<Metadata>(metadataStream, cancellationToken: stoppingToken);
    }

    public class Release
    {
        public required string HtmlUrl { get; init; }
        public required string Name { get; init; }
        public required IReadOnlyCollection<Asset> Assets { get; init; }
    }

    public class Asset
    {
        public required string Name { get; init; }
        public required string BrowserDownloadUrl { get; init; }
    }

    class Metadata
    {
        public required string GameVersion { get; init; }
    }

    [GeneratedRegex("<(?<uri>[^>]*)>; rel=\"next\"")]
    private static partial Regex NextLinkRegex();
}
