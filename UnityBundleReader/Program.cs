using System.Collections.Specialized;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;
using CommandLine;
using CommandLine.Text;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using Serilog.Extensions.Logging;
using UnityBundleReader;
using UnityBundleReader.Classes;
using ILogger = Microsoft.Extensions.Logging.ILogger;
using Logger = Serilog.Core.Logger;
using Object = UnityBundleReader.Classes.Object;

#if DEBUG
const LogEventLevel defaultLoggingLevel = LogEventLevel.Debug;
#else
const LogEventLevel defaultLoggingLevel = LogEventLevel.Information;
#endif
const string outputTemplate = "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj} ({SourceContext}){NewLine}{Exception}";
Logger logger = new LoggerConfiguration().WriteTo.Console(outputTemplate: outputTemplate).MinimumLevel.Is(defaultLoggingLevel).CreateLogger();
Log.Logger = logger;
SerilogLoggerFactory loggerFactory = new(logger);

try
{
    ILogger log = loggerFactory.CreateLogger("UnityBundleReader");
    Parser parser = new(
        with =>
        {
            with.HelpWriter = null;
            with.AutoHelp = true;
            with.AutoVersion = true;
        }
    );
    ParserResult<object>? parserResult = parser.ParseArguments<LineArgs, ExtractArgs>(args);
    parserResult?.WithParsed<LineArgs>(ListCommand)
        .WithParsed<ExtractArgs>(ExtractCommand)
        .WithNotParsed(
            _ =>
            {
                Console.WriteLine(
                    HelpText.AutoBuild(
                        parserResult,
                        h =>
                        {
                            h.AdditionalNewLineAfterOption = false;
                            h.AutoHelp = true;
                            h.AutoVersion = true;
                            h.AddNewLineBetweenHelpSections = true;
                            return HelpText.DefaultParsingErrorsHandler(parserResult, h);
                        },
                        e => e
                    )
                );
            }
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

void ListCommand(LineArgs args)
{
    ILogger log = loggerFactory.CreateLogger("List");

    log.LogInformation("Loading bundles from paths: {Paths}.", args.BundlePaths);
    string[] behaviourNames = GetMonoBehaviors(args.BundlePaths).Select(m => m.MName).ToArray();

    log.LogInformation("- Found {Count} behaviours in bundle", behaviourNames.Length);
    foreach (string name in behaviourNames)
    {
        log.LogInformation("\t- {Name}", name);
    }
}

void ExtractCommand(ExtractArgs args)
{
    ILogger log = loggerFactory.CreateLogger("Extract");
    string[] fields = args.Fields.SelectMany(s => s.Split(',')).ToArray();

    log.LogInformation("Loading bundles from paths: {Paths}.", args.BundlePaths);
    MonoBehaviour[] behaviours = GetMonoBehaviors(args.BundlePaths).ToArray();

    log.LogInformation("- Found {Count} behaviours in bundle", behaviours.Length);

    int count = 0;
    foreach (MonoBehaviour behaviour in behaviours)
    {
        string basePath = Path.GetFullPath(args.OutputPath);
        string directory = Path.Join(basePath, Path.GetFileNameWithoutExtension(behaviour.AssetsFile.OriginalPath));
        string path = Path.Join(directory, $"{behaviour.MName}.json");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        string json;
        try
        {
            json = ExtractPropertiesOfBehaviour(behaviour, fields);
        }
        catch (Exception exn)
        {
            log.LogError(exn, "Could not extract properties of behaviour {Name}.", behaviour.MName);
            continue;
        }

        File.WriteAllText(path, json);
        log.LogInformation("\t\t- MonoBehaviour {Name} saved at {Path}", behaviour.MName, path);
        count++;
    }

    log.LogInformation("Extracted {Count}/{TotalCount} behaviours.", count, behaviours.Length);
}

string[] GetBundlePaths(IEnumerable<string> inputs)
{
    List<string> result = [];

    foreach (string input in inputs)
    {
        if (Directory.Exists(input))
        {
            result.AddRange(Directory.EnumerateFiles(input, "*.bundle"));
        }
        else
        {
            result.Add(input);
        }
    }

    return result.ToArray();
}

IEnumerable<MonoBehaviour> GetMonoBehaviors(IEnumerable<string> inputs)
{
    string[] bundlePaths = GetBundlePaths(inputs);
    if (bundlePaths.Length == 0)
    {
        yield break;
    }

    AssetsManager assetsManager = new() { SpecifyUnityVersion = "2022.3.29f1" };
    assetsManager.LoadFiles(bundlePaths);

    foreach (Object obj in assetsManager.AssetsFileList.SelectMany(file => file.Objects))
    {
        switch (obj)
        {
            case MonoBehaviour monoBehaviour:
                yield return monoBehaviour;
                break;
        }
    }
}

string ExtractPropertiesOfBehaviour(MonoBehaviour monoBehaviour, string[] fields)
{
    OrderedDictionary properties = monoBehaviour.ToType();

    string json1;
    if (fields.Length > 0)
    {
        Dictionary<string, object?> toWrite = ExtractPropertiesOfDictionary(properties, fields);
        json1 = JsonSerializer.Serialize(toWrite, new JsonSerializerOptions { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals });
    }
    else
    {
        json1 = JsonSerializer.Serialize(properties, new JsonSerializerOptions { WriteIndented = true, NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals });
    }
    return json1;
}

Dictionary<string, object?> ExtractPropertiesOfDictionary(OrderedDictionary properties, string[] fields)
{
    Dictionary<string, object?> result = new();

    foreach (object? key in properties.Keys)
    {
        if (key is not string propName || fields.All(f => !Like(propName, f)))
        {
            continue;
        }

        result[propName] = properties[propName];
    }

    return result;
}

bool Like(string str, string pattern)
{
    return new Regex("^" + Regex.Escape(pattern).Replace(@"\*", ".*").Replace(@"\?", ".") + "$", RegexOptions.IgnoreCase | RegexOptions.Singleline).IsMatch(str);
}

namespace UnityBundleReader
{
    [Verb("list", HelpText = "List all the assets in the bundles.")]
    class LineArgs
    {
        [Value(0, Min = 1, MetaName = "bundles", HelpText = "Bundle files.")]
        public IEnumerable<string> BundlePaths { get; set; } = [];

        [Option('v', "verbose", Default = false, HelpText = "Print more stuff.")]
        public bool Verbose { get; set; }
    }


    [Verb("extract", HelpText = "Extract all the assets in the bundles.")]
    class ExtractArgs
    {
        [Value(0, Min = 1, MetaName = "bundles", HelpText = "Bundle files.")]
        public IEnumerable<string> BundlePaths { get; set; } = [];

        [Option('f', "field", HelpText = "If set, fields to extract. Glob patterns are accepted.")]
        public IEnumerable<string> Fields { get; set; } = [];

        [Option('o', "output", Default = "./output", HelpText = "Output directory.")]
        public string OutputPath { get; set; } = "";

        [Option('v', "verbose", Default = false, HelpText = "Print more stuff.")]
        public bool Verbose { get; set; }
    }
}