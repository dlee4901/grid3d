using System.Collections.Generic;

public struct RangeConfig
{
    public int Start { get; set; }
    public int End { get; set; }
}

public class PositionRangeConfig
{
    public List<int> Positions { get; set; } = new();
    public List<RangeConfig> Ranges { get; set; } = new();
    
    public string Type { get; set; } = "";
}

public class MapConfig : INameId
{
    public string Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int MaxTeamCost { get; set; }
    
    public int PlayerCount { get; set; } = 2;
    public List<PositionRangeConfig> Terrain { get; set; }
    public List<PositionRangeConfig> PlayerStartPositions { get; set; }
    public List<PositionRangeConfig> EntityStartPositions { get; set; }
}