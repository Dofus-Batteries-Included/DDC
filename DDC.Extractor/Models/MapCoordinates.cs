using System.Collections.Generic;

namespace DDC.Extractor.Models;

public class MapCoordinates
{
    public Position Position { get; init; }
    public IReadOnlyCollection<long> MapIds { get; init; }
}
