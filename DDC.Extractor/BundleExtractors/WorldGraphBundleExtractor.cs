using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using DDC.Extractor.Models;
using UnityBundleReader.Classes;

namespace DDC.Extractor.BundleExtractors;

public class WorldGraphBundleExtractor : IBundleExtractor<WorldGraph>
{
    public WorldGraph Extract(IReadOnlyCollection<MonoBehaviour> behaviours)
    {
        MonoBehaviour behaviour = behaviours.FirstOrDefault(b => string.Equals(b.Name, "WorldGraph", StringComparison.OrdinalIgnoreCase));
        OrderedDictionary props = behaviour?.ToType();
        if (props == null)
        {
            return null;
        }

        object verticesObj = props["m_vertices"];
        object edgesObj = props["m_edges"];

        if (verticesObj is not IList wgVertices || edgesObj is not IList wgEdges)
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

    static List<WorldGraphNode> ExtractNodes(IList vertices)
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
                    Id = vert["m_uid"] as int? ?? 0,
                    MapId = vert["m_mapId"] as long? ?? 0,
                    ZoneId = vert["m_zoneId"] as int? ?? 0
                }
            );
        }

        return result;
    }

    static List<WorldGraphEdge> ExtractEdges(IList edges)
    {
        List<WorldGraphEdge> result = [];

        foreach (object edgeObj in edges)
        {
            if (edgeObj is not IDictionary edge || edge["m_from"] is not IDictionary from || edge["m_to"] is not IDictionary to || edge["m_transitions"] is not IList transitions)
            {
                continue;
            }

            IDictionary transition = transitions.Count == 0 ? null : transitions[0] as IDictionary;

            if (transitions.Count > 1)
            {
                throw new Exception($"Found edge with multiple transitions: {from["m_uid"]} -> {to["m_uid"]}");
            }

            result.Add(
                new WorldGraphEdge
                {
                    From = from["m_uid"] as int? ?? 0,
                    To = to["m_uid"] as int? ?? 0,
                    Types = GetEdgeTypes((WorldGraphEdgeTypeFlags)(transition?["m_type"] as int? ?? 0)),
                    Direction = (WorldGraphEdgeDirection)(transition?["m_direction"] as int? ?? 0)
                }
            );
        }

        return result;
    }

    static IReadOnlyCollection<WorldGraphEdgeType> GetEdgeTypes(WorldGraphEdgeTypeFlags flags)
    {
        var result = new List<WorldGraphEdgeType>();
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.Scroll))
        {
            result.Add(WorldGraphEdgeType.Scroll);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.ScrollAction))
        {
            result.Add(WorldGraphEdgeType.ScrollAction);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.MapEvent))
        {
            result.Add(WorldGraphEdgeType.MapEvent);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.MapAction))
        {
            result.Add(WorldGraphEdgeType.MapAction);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.MapObstacle))
        {
            result.Add(WorldGraphEdgeType.MapObstacle);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.Interactive))
        {
            result.Add(WorldGraphEdgeType.Interactive);
        }
        
        if (flags.HasFlag(WorldGraphEdgeTypeFlags.NpcAction))
        {
            result.Add(WorldGraphEdgeType.NpcAction);
        }

        return result;
    }

    /// <summary>
    ///     Type of <see cref="Core.PathFinding.WorldPathfinding.Transition.m_type" />
    /// </summary>
    [Flags]
    enum WorldGraphEdgeTypeFlags
    {
        Unspecified = 0,
        Scroll = 1,
        ScrollAction = 2,
        MapEvent = 4,
        MapAction = 8,
        MapObstacle = 16, // 0x00000010
        Interactive = 32, // 0x00000020
        NpcAction = 64 // 0x00000040
    }
}
