using System.Collections;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using DofusBundleReader.Abstractions;
using DofusBundleReader.Maps.Models;
using Microsoft.Extensions.Logging;
using UnityBundleReader.Classes;

namespace DofusBundleReader.Maps;

public partial class MapsBundleExtractor : IBundleExtractor<Dictionary<long, Map>>
{
    readonly ILogger<MapsBundleExtractor> _logger;

    public MapsBundleExtractor(ILogger<MapsBundleExtractor> logger)
    {
        _logger = logger;
    }

    public Dictionary<long, Map>? Extract(IReadOnlyCollection<MonoBehaviour> behaviours)
    {
        Dictionary<long, Map> result = new();
        Regex nameRegex = MapBehaviourNameRegex();

        int count = 0;
        int errors = 0;
        foreach (MonoBehaviour behaviour in behaviours.Where(b => b.Name != null))
        {
            try
            {
                Match match = nameRegex.Match(behaviour.Name!);
                if (!match.Success)
                {
                    continue;
                }

                _logger.LogDebug("Reading data from {Name}. {Percent}% ({Count}/{TotalCount}).", behaviour.Name, count * 100.0 / behaviours.Count, count, behaviours.Count);

                string idStr = match.Groups["id"].Value;
                if (!long.TryParse(idStr, out long id))
                {
                    continue;
                }

                Map? map = ExtractMap(behaviour);
                if (map == null)
                {
                    continue;
                }

                result[id] = map;
            }
            catch (Exception exn)
            {
                _logger.LogError(exn, "An error occured while extracting map data from {Name}.", behaviour.Name);
                errors++;
            }

            count++;
        }

        _logger.LogInformation("Maps extraction over: {SuccessCount} successes, {ErrorCount} errors.", result.Count, errors);

        return result.Count == 0 ? null : result;
    }

    static Map? ExtractMap(MonoBehaviour behaviour)
    {
        OrderedDictionary? props = behaviour.ToType();
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
