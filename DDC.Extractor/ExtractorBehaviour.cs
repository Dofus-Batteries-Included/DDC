using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BepInEx.Unity.IL2CPP.Utils.Collections;
using Core.DataCenter;
using Core.DataCenter.Metadata.Effect;
using Core.DataCenter.Metadata.Interactive;
using Core.DataCenter.Metadata.Item;
using Core.DataCenter.Metadata.Job;
using Core.DataCenter.Metadata.Quest.TreasureHunt;
using Core.DataCenter.Metadata.World;
using Core.Localization;
using DDC.Extractor.Converters;
using DDC.Extractor.Models.Effects;
using JsonSubTypes;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using UnityEngine;
using EffectInstance = Core.DataCenter.Metadata.Effect.EffectInstance;

namespace DDC.Extractor;

public class ExtractorBehaviour : MonoBehaviour
{
    static readonly JsonSerializerSettings JsonSerializerSettings = new()
    {
        ContractResolver = new OrderedContractResolver { NamingStrategy = new KebabCaseNamingStrategy() },
        Converters =
        [
            new StringEnumConverter(new KebabCaseNamingStrategy()),
            JsonSubtypesConverterBuilder.Of(typeof(EffectInstance), "type")
                .RegisterSubtype(typeof(EffectInstanceString), "string")
                .RegisterSubtype(typeof(EffectInstanceDice), "dice")
                .RegisterSubtype(typeof(EffectInstanceInteger), "integer")
                .RegisterSubtype(typeof(EffectInstanceMinMax), "min-max")
                .RegisterSubtype(typeof(EffectInstanceDate), "date")
                .RegisterSubtype(typeof(EffectInstanceDuration), "duration")
                .RegisterSubtype(typeof(EffectInstanceLadder), "ladder")
                .RegisterSubtype(typeof(EffectInstanceMount), "mount")
                .RegisterSubtype(typeof(EffectInstanceCreature), "creature")
                .SerializeDiscriminatorProperty()
                .Build()
        ],
        NullValueHandling = NullValueHandling.Ignore
    };

    void Start() => StartCoroutine(StartCoroutine().WrapToIl2Cpp());

    static IEnumerator StartCoroutine()
    {
        yield return Wait(1);

        Extractor.Logger.LogInfo("Start extracting data...");

        yield return WaitForCompletion(ExtractDataFromGame("point-of-interest.json", DataCenterModule.GetDataRoot<PointOfInterestRoot>(), new PointsOfInterestConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("map-positions.json", DataCenterModule.GetDataRoot<MapPositionsRoot>(), new MapPositionsConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("map-coordinates.json", DataCenterModule.GetDataRoot<MapCoordinatesRoot>(), new MapCoordinatesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("areas.json", DataCenterModule.GetDataRoot<AreasRoot>(), new AreasConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("super-areas.json", DataCenterModule.GetDataRoot<SuperAreasRoot>(), new SuperAreasConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("sub-areas.json", DataCenterModule.GetDataRoot<SubAreasRoot>(), new SubAreasConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("world-maps.json", DataCenterModule.GetDataRoot<WorldMapsRoot>(), new WorldMapsConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("interactives.json", DataCenterModule.GetDataRoot<InteractivesRoot>(), new InteractivesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("items.json", DataCenterModule.GetDataRoot<ItemsRoot>(), new ItemsConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("item-sets.json", DataCenterModule.GetDataRoot<ItemSetsRoot>(), new ItemSetsConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("item-types.json", DataCenterModule.GetDataRoot<ItemTypesRoot>(), new ItemTypesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("item-super-types.json", DataCenterModule.GetDataRoot<ItemSuperTypesRoot>(), new ItemSuperTypesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("evolutive-item-types.json", DataCenterModule.GetDataRoot<EvolutiveItemTypesRoot>(), new EvolutiveItemTypesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("effects.json", DataCenterModule.GetDataRoot<EffectsRoot>(), new EffectsConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("recipes.json", DataCenterModule.GetDataRoot<RecipesRoot>(), new RecipesConverter()));
        yield return WaitForCompletion(ExtractDataFromGame("jobs.json", DataCenterModule.GetDataRoot<JobsRoot>(), new JobsConverter()));
        yield return WaitForCompletion(ExtractLocale("de.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/de.bin"));
        yield return WaitForCompletion(ExtractLocale("en.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/en.bin"));
        yield return WaitForCompletion(ExtractLocale("es.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/es.bin"));
        yield return WaitForCompletion(ExtractLocale("fr.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/fr.bin"));
        yield return WaitForCompletion(ExtractLocale("pt.i18n.json", "Dofus_Data/StreamingAssets/Content/I18n/pt.bin"));

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
        string serialized = JsonConvert.SerializeObject(localizationTable, JsonSerializerSettings);
        await File.WriteAllTextAsync(path, serialized);

        Extractor.Logger.LogInfo($"Extracted locale {table.m_header.languageCode} to {path}.");
    }

    static async Task ExtractDataFromGame<TData, TSerializedData>(string filename, MetadataRoot<TData> root, IConverter<TData, TSerializedData> converter)
    {
        string dataTypeName = typeof(TData).Name;
        string path = Path.Join(Extractor.OutputDirectory, filename);

        Extractor.Logger.LogInfo($"Extracting data of type {dataTypeName}...");

        TSerializedData[] arr;
        try
        {
            Il2CppSystem.Collections.Generic.List<TData> data = root.GetObjects();
            arr = data._items.Take(data.Count).Select(converter.Convert).ToArray();
        }
        catch (Exception exn)
        {
            Extractor.Logger.LogError($"Error while converting data of type {dataTypeName}.{Environment.NewLine}{exn}");
            return;
        }

        if (arr.Length == 0)
        {
            Extractor.Logger.LogError($"Did not find any data of type {dataTypeName}.");
            return;
        }

        string serialized = JsonConvert.SerializeObject(arr, JsonSerializerSettings);
        await File.WriteAllTextAsync(path, serialized);

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

        if (task.IsFaulted)
        {
            throw new Exception("Task is faulted.", task.Exception);
        }

        if (task.IsCanceled)
        {
            throw new Exception("Task is canceled.", task.Exception);
        }
    }
}
