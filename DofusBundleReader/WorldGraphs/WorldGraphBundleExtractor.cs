using System.Collections;
using System.Collections.Specialized;
using System.Diagnostics.CodeAnalysis;
using DofusBundleReader.Abstractions;
using DofusBundleReader.WorldGraphs.Models;
using UnityBundleReader.Classes;

namespace DofusBundleReader.WorldGraphs;

public class WorldGraphBundleExtractor : IBundleExtractor<WorldGraph>
{
    public WorldGraph? Extract(IReadOnlyCollection<MonoBehaviour> behaviours)
    {
        MonoBehaviour? behaviour = behaviours.FirstOrDefault(b => string.Equals(b.Name, "WorldGraph", StringComparison.OrdinalIgnoreCase));
        OrderedDictionary? props = behaviour?.ToType();
        if (props == null)
        {
            return null;
        }

        object? verticesObj = props["m_vertices"];
        object? edgesObj = props["m_edges"];

        if (!TryConvertWeirdDictionary(verticesObj, out IReadOnlyCollection<object>? wgVertices) || !TryConvertWeirdDictionary(edgesObj, out IReadOnlyCollection<object>? wgEdges))
        {
            return null;
        }

        List<WorldGraphNode> nodes = ExtractNodes(wgVertices);
        List<WorldGraphEdge> edges = ExtractEdges(wgEdges);

        return new WorldGraph
        {
            Nodes = nodes,
            Edges = edges
        };
    }

    static List<WorldGraphNode> ExtractNodes(IReadOnlyCollection<object> vertices)
    {
        List<WorldGraphNode> result = [];

        foreach (object vertObj in vertices)
        {
            if (vertObj is not IDictionary vert)
            {
                continue;
            }

            result.Add(
                new WorldGraphNode
                {
                    Id = Convert.ToInt64(vert["m_uid"]),
                    MapId = Convert.ToInt64(vert["m_mapId"]),
                    ZoneId = Convert.ToInt32(vert["m_zoneId"])
                }
            );
        }

        return result;
    }

    static List<WorldGraphEdge> ExtractEdges(IReadOnlyCollection<object> edges)
    {
        List<WorldGraphEdge> result = [];

        foreach (object edgeObj in edges)
        {
            if (edgeObj is not IDictionary edge || edge["m_from"] is not IDictionary from || edge["m_to"] is not IDictionary to || edge["m_transitions"] is not IList transitions)
            {
                continue;
            }

            result.Add(
                new WorldGraphEdge
                {
                    From = Convert.ToInt64(from["m_uid"]),
                    To = Convert.ToInt64(to["m_uid"]),
                    Transitions = GetTransitions(transitions)
                }
            );
        }

        return result;
    }

    static WorldGraphEdgeTransition[]? GetTransitions(IList transitions)
    {
        WorldGraphEdgeTransition[] result = transitions.OfType<IDictionary>()
            .Select(
                t => new WorldGraphEdgeTransition
                {
                    MapId = Convert.ToInt64(t["m_transitionMapId"]),
                    Type = EnumValueOrDefault((WorldGraphEdgeType)Convert.ToInt32(t["m_type"])),
                    Direction = EnumValueOrDefault((WorldGraphEdgeDirection)Convert.ToInt32(t["m_direction"]))
                }
            )
            .Where(t => t.MapId != default || t.Type is not null || t.Direction is not null)
            .ToArray();
        return result.Length == 0 ? null : result;
    }

    static T? EnumValueOrDefault<T>(T value, T? defaultValue = default) where T: struct, Enum => Enum.IsDefined(value) ? value : defaultValue;

    static bool TryConvertWeirdDictionary(object? obj, [NotNullWhen(true)] out IReadOnlyCollection<object>? result)
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
}
