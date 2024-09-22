// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using DofusBundleReader.Abstractions;
using DofusBundleReader.Maps;
using DofusBundleReader.WorldGraphs;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using UnityBundleReader;
using UnityBundleReader.Classes;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Serilog.Core.Logger;

#if DEBUG
const LogEventLevel defaultLoggingLevel = LogEventLevel.Debug;
#else
const LogEventLevel defaultLoggingLevel = LogEventLevel.Information;
#endif
const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} ({SourceContext}){NewLine}{Exception}";
Logger logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: outputTemplate).MinimumLevel.Is(defaultLoggingLevel).CreateLogger();
Log.Logger = logger;
SerilogLoggerFactory loggerFactory = new(logger);

ILogger globalLogger = loggerFactory.CreateLogger("Global");
JsonSerializerOptions jsonSerializerOptions = new()
{
    PropertyNamingPolicy = JsonNamingPolicy.KebabCaseLower,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    Converters = { new JsonStringEnumConverter(JsonNamingPolicy.KebabCaseLower) }
};

try
{
    ParserResult<object> parserResult = Parser.Default.ParseArguments<ExtractWorldGraphArgs, ExtractMapsArgs>(args);

    await parserResult.WithParsedAsync<ExtractWorldGraphArgs>(
        args => ExtractDataFromBundles(Path.Join(args.Output, "world-graph.json"), args.BundleDirectory, "worldassets", new WorldGraphBundleExtractor())
    );

    await parserResult.WithParsedAsync<ExtractMapsArgs>(
        args => ExtractDataFromBundles(
            Path.Join(args.Output, "maps.json"),
            args.BundleDirectory,
            "mapdata",
            new MapsBundleExtractor(loggerFactory.CreateLogger<MapsBundleExtractor>())
        )
    );
}
catch (Exception exn)
{
    Log.Logger.Fatal(exn, "An unexpected error occured.");
}
finally
{
    await Log.CloseAndFlushAsync();
}

return;

async Task ExtractDataFromBundles<TData>(string output, string bundleFileDirectory, string bundleFilePrefix, IBundleExtractor<TData> extractor)
{
    string dataTypeName = typeof(TData).ToString();

    globalLogger.LogInformation("Extracting data of type {Name}...", dataTypeName);

    if (!Directory.Exists(bundleFileDirectory))
    {
        globalLogger.LogError("Could not find directory {Directory}.", bundleFileDirectory);
        return;
    }

    string[] files = Directory.EnumerateFiles(bundleFileDirectory).Where(p => Path.GetFileName(p).StartsWith(bundleFilePrefix)).ToArray();
    switch (files.Length)
    {
        case 0:
            globalLogger.LogError("Could not find bundle {Name} in directory {Directory}.", bundleFilePrefix, bundleFileDirectory);
            return;
        case 1:
            globalLogger.LogInformation("Found bundle file: {File}.", files.First());
            break;
        case > 1:
            globalLogger.LogInformation(
                "Found {Count} bundle files at {Directory}: {Files}.",
                files.Length,
                bundleFileDirectory,
                string.Join(", ", files.Select(Path.GetFileName))
            );
            break;
    }

    AssetsManager assetsManager = new(NullLogger<AssetsManager>.Instance) { SpecifyUnityVersion = "2022.3.29f1" };
    assetsManager.LoadFiles(files);
    MonoBehaviour[] behaviours = assetsManager.AssetsFileList.SelectMany(f => f.Objects).OfType<MonoBehaviour>().ToArray();

    TData? data = extractor.Extract(behaviours);
    if (data == null)
    {
        globalLogger.LogError("Could not extract data of type {Name}.", dataTypeName);
        return;
    }

    await using FileStream stream = File.Open(output, FileMode.Create);
    await JsonSerializer.SerializeAsync(stream, data, jsonSerializerOptions);
    stream.Flush();

    globalLogger.LogInformation("Extracted data of type {Name} to {Output}.", dataTypeName, output);
}

abstract class ExtractArgsBase
{
    [Value(0, Required = false, Default = ".", HelpText = "Directory containing the worldassets_*.bundle file.")]
    public string BundleDirectory { get; set; } = ".";

    [Option('o', "output", Default = "./output", HelpText = "Output directory.")]
    public string Output { get; set; } = "./output";
}

[Verb("worldgraph")]
class ExtractWorldGraphArgs : ExtractArgsBase
{
}

[Verb("maps")]
class ExtractMapsArgs : ExtractArgsBase
{
}
