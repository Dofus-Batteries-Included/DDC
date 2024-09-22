using MessagePack;

namespace DofusBundleReader.Maps.Models;

[MessagePackObject]
public class Cell
{
    [Key(0)]
    public int CellNumber { get; init; }

    [Key(1)]
    public int Floor { get; init; }

    [Key(2)]
    public int MoveZone { get; init; }

    [Key(3)]
    public int LinkedZone { get; init; }

    [Key(4)]
    public int Speed { get; init; }

    [Key(5)]
    public bool Los { get; init; }

    [Key(6)]
    public bool Visible { get; init; }

    [Key(7)]
    public bool NonWalkableDuringFight { get; init; }

    [Key(8)]
    public bool NonWalkableDuringRp { get; init; }

    [Key(9)]
    public bool HavenbagCell { get; init; }
}
