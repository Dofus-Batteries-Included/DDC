using System.Collections;
using System.Diagnostics.CodeAnalysis;
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
using UnityBundleReader.Extensions;
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

ILogger globalLogger = loggerFactory.CreateLogger("Global");
UnityBundleReader.Logger.Configure(globalLogger);

JsonSerializerOptions jsonSerializerOptions = new() { NumberHandling = JsonNumberHandling.AllowNamedFloatingPointLiterals };

try
{
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
    string[] behaviourNames = GetMonoBehaviors(args.BundlePaths).Select(m => m.Name ?? "__UNNAMED__").ToArray();

    log.LogInformation("- Found {Count} behaviours in bundle", behaviourNames.Length);
    foreach (string name in behaviourNames)
    {
        log.LogInformation("\t- {Name}", name);
    }
}

void ExtractCommand(ExtractArgs args)
{
    ILogger log = loggerFactory.CreateLogger("Extract");
    string[] behaviourNames = args.Behaviours.SelectMany(s => s.Split(',')).ToArray();
    string[] fieldNames = args.Fields.SelectMany(s => s.Split(',')).ToArray();
    string[] removeFieldPrefixes = args.RemoveFieldPrefixes.SelectMany(s => s.Split(',')).ToArray();

    log.LogInformation("Loading bundles from paths: {Paths}.", args.BundlePaths);
    MonoBehaviour[] behaviours = GetMonoBehaviors(args.BundlePaths).Where(b => behaviourNames.Any(p => Like(b.Name, p))).ToArray();

    log.LogInformation("- Found {Count} behaviours in bundle", behaviours.Length);

    int count = 0;
    foreach (MonoBehaviour behaviour in behaviours)
    {

        string basePath = Path.GetFullPath(args.OutputPath);
        string directory = Path.Join(basePath, Path.GetFileNameWithoutExtension(behaviour.AssetsFile.OriginalPath));
        string path = Path.Join(directory, $"{behaviour.Name}.json");
        if (!Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }

        log.LogDebug("- Extracting {Name} at {Path}...", behaviour.Name, path);

        string json;
        try
        {
            json = ExtractPropertiesOfBehaviour(behaviour, fieldNames, removeFieldPrefixes);
        }
        catch (Exception exn)
        {
            log.LogError(exn, "Could not extract properties of behaviour {Name}.", behaviour.Name);
            continue;
        }

        File.WriteAllText(path, json);
        log.LogInformation("\t\t- MonoBehaviour {Name} saved at {Path}", behaviour.Name, path);
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

    ILogger<AssetsManager> log = loggerFactory.CreateLogger<AssetsManager>();
    AssetsManager assetsManager = new(log) { SpecifyUnityVersion = "2022.3.29f1" };
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

string ExtractPropertiesOfBehaviour(MonoBehaviour monoBehaviour, string[] fields, string[] removeFieldPrefixes)
{
    Dictionary<string, object?> properties = monoBehaviour.ToType()?.ToDictionary(k => k as string ?? "", v => v) ?? [];

    if (fields.Length > 0)
    {
        properties = ExtractPropertiesOfDictionary(properties, fields);
    }

    foreach (KeyValuePair<string, object?> entry in properties)
    {
        if (TryConvertWeirdDictionary(entry.Value, out IReadOnlyCollection<object>? dict))
        {
            properties[entry.Key] = dict;
        }
    }

    if (removeFieldPrefixes.Length > 0)
    {
        properties = RemovePrefixes(properties, removeFieldPrefixes);
    }

    return JsonSerializer.Serialize(properties, jsonSerializerOptions);
}

Dictionary<string, object?> ExtractPropertiesOfDictionary(Dictionary<string, object?> properties, string[] fields)
{
    Dictionary<string, object?> result = new();

    foreach (string key in properties.Keys.Where(key => fields.Any(f => Like(key, f))))
    {
        result[key] = properties[key];
    }

    return result;
}

T RemovePrefixes<T>(T obj, string[] prefixesToRemove)
{
    switch (obj)
    {
        case IDictionary dictionary:
            object[] keys = dictionary.Keys.Cast<object>().ToArray();
            foreach (object key in keys)
            {
                object? newValue = RemovePrefixes(dictionary[key], prefixesToRemove);

                if (key is string str)
                {
                    string newKey = RemoveFieldPrefixes(str, prefixesToRemove);
                    dictionary[newKey] = newValue;
                    dictionary.Remove(key);
                }
                else
                {
                    dictionary[key] = newValue;
                }
            }
            break;
        case IList list:
            for (int i = 0; i < list.Count; i++)
            {
                list[i] = RemovePrefixes(list[i], prefixesToRemove);
            }
            break;
    }

    return obj;
}

string RemoveFieldPrefixes(string fieldName, string[] prefixesToRemove)
{
    bool atLeastOne;
    string result = fieldName;
    do
    {
        atLeastOne = false;
        foreach (string prefix in prefixesToRemove)
        {
            if (result.StartsWith(prefix))
            {
                result = result[prefix.Length..];
            }
        }
    } while (atLeastOne);

    return result;
}

bool TryConvertWeirdDictionary(object? obj, [NotNullWhen(true)] out IReadOnlyCollection<object>? result)
{
    result = null;

    if (obj is not IDictionary { Keys.Count: 2 } dictionary || !dictionary.Contains("m_keys") || !dictionary.Contains("m_values"))
    {
        return false;
    }

    object? keysObj = dictionary["m_keys"];
    object? valuesObj = dictionary["m_values"];
    if (keysObj is not IList keys || valuesObj is not IList values || keys.Count != values.Count)
    {
        return false;
    }

    List<object> list = [];

    for (int i = 0; i < keys.Count; i++)
    {
        object? key = keys[i];
        if (key == null)
        {
            continue;
        }

        object? value = values[i];
        if (TryConvertWeirdDictionary(value, out IReadOnlyCollection<object>? innerValues))
        {
            list.AddRange(innerValues);
        }
        else if (value is not null)
        {
            list.Add(value);
        }
    }

    result = list;
    return true;
}

bool Like(string? str, string pattern)
{
    if (str == null)
    {
        return false;
    }

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

        [Option('b', "behaviours", HelpText = "Behaviours to export. If not set, all behaviours will be exported. Glob patterns are accepted.")]
        public IEnumerable<string> Behaviours { get; set; } = [];

        [Option('f', "field", HelpText = "Fields to export. If not set, all fields will be exported. Glob patterns are accepted.")]
        public IEnumerable<string> Fields { get; set; } = [];

        [Option("remove-field-prefix", HelpText = "Prefixes to remove from field names.")]
        public IEnumerable<string> RemoveFieldPrefixes { get; set; } = [];

        [Option('o', "output", Default = "./output", HelpText = "Output directory.")]
        public string OutputPath { get; set; } = "";

        [Option('v', "verbose", Default = false, HelpText = "Print more stuff.")]
        public bool Verbose { get; set; }
    }
}
