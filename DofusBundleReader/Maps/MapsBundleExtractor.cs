using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using DofusBundleReader.Abstractions;
using DofusBundleReader.Maps.Models;
using Microsoft.Extensions.Logging;
using UnityBundleReader.Classes;

namespace DofusBundleReader.Maps;

public partial class MapsBundleExtractor : IBundleExtractor<IReadOnlyDictionary<long, Map>>
{
    readonly ILogger<MapsBundleExtractor> _logger;

    public MapsBundleExtractor(ILogger<MapsBundleExtractor> logger)
    {
        _logger = logger;
    }

    public IReadOnlyDictionary<long, Map>? Extract(IReadOnlyList<MonoBehaviour> behaviours)
    {
        Dictionary<long, Map> result = new();

        for (int index = 0; index < behaviours.Count; index++)
        {
            MonoBehaviour behaviour = behaviours[index];
            ExtractOne(behaviour, result);
            _logger.LogDebug("Processing map {Name}. {Percent:0.}% ({Count}/{TotalCount})", behaviour.Name, index * 100.0 / behaviours.Count, index, behaviours.Count);
        }

        _logger.LogInformation("Maps extraction over: {SuccessCount} successes, {ErrorCount} errors.", result.Count, behaviours.Count - result.Count);

        return result.Count == 0 ? null : result;
    }

    void ExtractOne(MonoBehaviour behaviour, Dictionary<long, Map> result)
    {
        Regex nameRegex = MapBehaviourNameRegex();

        try
        {
            Match match = nameRegex.Match(behaviour.Name!);
            if (!match.Success)
            {
                return;
            }

            string idStr = match.Groups["id"].Value;
            if (!long.TryParse(idStr, out long id))
            {
                _logger.LogWarning("Map ID is invalid: {Name}.", behaviour.Name);
                return;
            }

            Map? map = ExtractMap(behaviour);
            if (map == null)
            {
                _logger.LogWarning("Could not extract map from {Name}.", behaviour.Name);
                return;
            }

            result.TryAdd(id, map);
        }
        catch (Exception exn)
        {
            _logger.LogError(exn, "An error occured while extracting map data from {Name}.", behaviour.Name);
        }
    }

    static Map? ExtractMap(MonoBehaviour behaviour)
    {
        OrderedDictionary? props = behaviour.ToType(new HashSet<string> { "cellsData" });
        object? cellsDataObj = props?["cellsData"];
        if (cellsDataObj is not IList cellsData)
        {
            return null;
        }

        Dictionary<int, Cell> cells = ExtractCells(cellsData);

        return new Map
        {
            Cells = cells
        };
    }

    static Dictionary<int, Cell> ExtractCells(IList cellsData)
    {
        Dictionary<int, Cell> result = new();

        foreach (IDictionary cell in cellsData.OfType<IDictionary>())
        {
            int cellNumber = Convert.ToInt32(cell["cellNumber"]);
            result.Add(
                cellNumber,
                new Cell
                {
                    CellNumber = cellNumber,
                    Floor = Convert.ToInt32(cell["floor"]),
                    MoveZone = Convert.ToInt32(cell["moveZone"]),
                    LinkedZone = Convert.ToInt32(cell["linkedZone"]),
                    Speed = Convert.ToInt32(cell["speed"]),
                    Los = Convert.ToBoolean(cell["los"]),
                    Visible = Convert.ToBoolean(cell["nonWalkableDuringRP"]),
                    NonWalkableDuringFight = Convert.ToBoolean(cell["nonWalkableDuringFight"]),
                    NonWalkableDuringRp = Convert.ToBoolean(cell["nonWalkableDuringRP"]),
                    HavenbagCell = Convert.ToBoolean(cell["havenbagCell"])
                }
            );
        }

        return result;
    }

    [GeneratedRegex("map_(?<id>\\d+)")]
    private static partial Regex MapBehaviourNameRegex();
}
