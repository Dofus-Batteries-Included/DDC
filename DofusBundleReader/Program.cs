// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using System.Text.Json.Serialization;
using CommandLine;
using DofusBundleReader.Abstractions;
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
    ParserResult<object> parserResult = Parser.Default.ParseArguments<ExtractWorldGraphArgs, VersionArgs>(args);

    await parserResult.WithParsedAsync<ExtractWorldGraphArgs>(
        args => ExtractDataFromBundle(Path.Join(args.Output, "world-graph.json"), args.BundleDirectory, "worldassets", new WorldGraphBundleExtractor())
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

async Task ExtractDataFromBundle<TData>(string output, string bundleFileDirectory, string bundleFilePrefix, IBundleExtractor<TData> extractor)
{
    string dataTypeName = typeof(TData).Name;

    globalLogger.LogInformation("Extracting data of type {Name}...", dataTypeName);

    if (!Directory.Exists(bundleFileDirectory))
    {
        globalLogger.LogError("Could not find directory {Directory}.", bundleFileDirectory);
        return;
    }

    string? file = Directory.EnumerateFiles(bundleFileDirectory).FirstOrDefault(p => Path.GetFileName(p).StartsWith(bundleFilePrefix));
    if (file == null)
    {
        globalLogger.LogError("Could not find bundle {Name} in directory {Directory}.", bundleFilePrefix, bundleFileDirectory);
        return;
    }

    globalLogger.LogInformation("Found bundle file: {File}.", file);

    AssetsManager assetsManager = new(NullLogger<AssetsManager>.Instance) { SpecifyUnityVersion = "2022.3.29f1" };
    assetsManager.LoadFiles(file);
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

[Verb("worldgraph")]
class ExtractWorldGraphArgs
{
    [Value(0, Required = false, Default = ".", HelpText = "Directory containing the worldassets_*.bundle file.")]
    public string BundleDirectory { get; set; } = ".";

    [Option('o', "output", Default = "./output", HelpText = "Output directory.")]
    public string Output { get; set; } = "./output";
}

[Verb("version")]
class VersionArgs
{
}
