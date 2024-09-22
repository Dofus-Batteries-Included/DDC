using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Core.DataCenter;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using Core.DataCenter.Metadata.World;
using Core.Localization;
using DDC.Extractor.BundleExtractors;
using DDC.Extractor.Converters;
using Microsoft.Extensions.Logging.Abstractions;
using UnityBundleReader;
using UnityEngine;

namespace DDC.Extractor;

public class ExtractorBehaviour : MonoBehaviour
{
    static readonly JsonSerializerOptions JsonSerializerOptions = new(JsonSerializerDefaults.Web) { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull };

    void Start() => StartCoroutine(StartCoroutine().WrapToIl2Cpp());

    static IEnumerator StartCoroutine()
    {
        yield return Wait(1);

        Extractor.Logger.LogInfo("Start extracting data...");

        yield return WaitForCompletion(ExtractDataFromGame("point-of-interest.json", DataCenterModule.GetDataRoot<PointOfInterestRoot>(), new PointOfInterestConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("map-positions.json", DataCenterModule.GetDataRoot<MapPositionsRoot>(), new MapPositionsConverter()));
        yield return WaitForCompletion(ExtractLocale("de.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/de.bin"));
        yield return WaitForCompletion(ExtractLocale("en.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/en.bin"));
        yield return WaitForCompletion(ExtractLocale("es.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/es.bin"));
        yield return WaitForCompletion(ExtractLocale("fr.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/fr.bin"));
        yield return WaitForCompletion(ExtractLocale("pt.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/pt.bin"));
        yield return WaitForCompletion(
            ExtractDataFromBundle("world-graph.json", "Dofus_Data/StreamingAssets/aa/StandaloneWindows64", "worldassets", new WorldGraphBundleExtractor())
        );

        Extractor.Logger.LogInfo("DDC data extraction complete.");

        Application.Quit(0);
    }

    static async Task ExtractLocale(string filename, string binFile)
    {
        Extractor.Logger.LogInfo($"Extracting locale from {binFile}...");
        LocalizationTable table = LocalizationTable.ReadFrom(binFile);

        Dictionary<int, string> entries = new();
        foreach (Il2CppSystem.Collections.Generic.KeyValuePair<int, uint> entry in table.m_header.m_integerKeyedOffsets)
        {
            if (!table.TryLookup(entry.Key, out string output))
            {
                continue;
            }

            entries[entry.Key] = output;
        }

        Models.I18N.LocalizationTable localizationTable = new() { LanguageCode = table.m_header.languageCode, Entries = entries };

        string path = Path.Join(Extractor.OutputDirectory, filename);
        await using FileStream stream = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(stream, localizationTable, JsonSerializerOptions);
        stream.Flush();

        Extractor.Logger.LogInfo($"Extracted locale {table.m_header.languageCode} to {path}.");
    }

    static async Task ExtractDataFromGame<TData, TSerializedData>(string filename, MetadataRoot<TData> root, IConverter<TData, TSerializedData> converter)
    {
        string dataTypeName = typeof(TData).Name;
        string path = Path.Join(Extractor.OutputDirectory, filename);

        Extractor.Logger.LogInfo($"Extracting data of type {dataTypeName}...");

        Il2CppSystem.Collections.Generic.List<TData> data = root.GetObjects();
        TSerializedData[] arr = data._items.Take(data.Count).Select(converter.Convert).ToArray();

        await using FileStream stream = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(stream, arr, JsonSerializerOptions);
        stream.Flush();

        Extractor.Logger.LogInfo($"Extracted data of type {dataTypeName} to {path}.");
    }

    static async Task ExtractDataFromBundle<TData>(string filename, string bundleFileDirectory, string bundleFilePrefix, IBundleExtractor<TData> extractor)
    {
        string dataTypeName = typeof(TData).Name;
        string path = Path.Join(Extractor.OutputDirectory, filename);

        Extractor.Logger.LogInfo($"Extracting data of type {dataTypeName}...");

        if (!Directory.Exists(bundleFileDirectory))
        {
            Extractor.Logger.LogError($"Could not find directory {bundleFileDirectory}.");
            return;
        }

        string file = Directory.EnumerateFiles(bundleFileDirectory).FirstOrDefault(p => Path.GetFileName(p).StartsWith(bundleFilePrefix));
        if (file == null)
        {
            Extractor.Logger.LogError($"Could not find bundle {bundleFilePrefix} in directory {bundleFileDirectory}.");
            return;
        }

        Extractor.Logger.LogInfo($"Found bundle file: {file}.");

        AssetsManager assetsManager = new(NullLogger<AssetsManager>.Instance) { SpecifyUnityVersion = "2022.3.29f1" };
        assetsManager.LoadFiles(file);
        UnityBundleReader.Classes.MonoBehaviour[] behaviours = assetsManager.AssetsFileList.SelectMany(f => f.Objects).OfType<UnityBundleReader.Classes.MonoBehaviour>().ToArray();

        TData data = extractor.Extract(behaviours);

        await using FileStream stream = File.OpenWrite(path);
        await JsonSerializer.SerializeAsync(stream, data, JsonSerializerOptions);
        stream.Flush();

        Extractor.Logger.LogInfo($"Extracted data of type {dataTypeName} to {path}.");
    }

    static IEnumerator Wait(float seconds)
    {
        float startTime = Time.time;
        while (Time.time - startTime < seconds)
        {
            yield return null;
        }
    }

    static IEnumerator WaitForCompletion(Task task)
    {
        while (!task.IsCompleted)
        {
            yield return null;
        }
    }
}
