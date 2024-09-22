using System.Collections.Generic;
using DDC.Extractor.Common.Models;

namespace DDC.Extractor.MapCoordinates.Models;

public class MapCoordinates
{
    public Position Position { get; init; }
    public IReadOnlyCollection<long> MapIds { get; init; }
}
