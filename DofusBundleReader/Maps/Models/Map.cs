namespace DofusBundleReader.Maps.Models;

public class Map
{
    public required Dictionary<int, Cell> Cells { get; init; }
}

public class Cell
{
    public int CellNumber { get; set; }
    public int Floor { get; set; }
    public int MoveZone { get; set; }
    public int LinkedZone { get; set; }
    public int Speed { get; set; }
    public bool Los { get; set; }
    public bool Visible { get; set; }
    public bool NonWalkableDuringFight { get; set; }
    public bool NonWalkableDuringRp { get; set; }
    public bool HavenbagCell { get; set; }
}
