using System.Collections.Generic;

public class RangeConfig
{
    public int Start { get; set; }
    public int End { get; set; }
}

public class PositionConfig
{
    public List<int> Values { get; set; } = new();
    public List<RangeConfig> Ranges { get; set; } = new();
}

public class TerrainConfig
{
    public string Type { get; set; }
    public PositionConfig Positions { get; set; } = new();
}

public class EntityStartConfig
{
    public string EntityId { get; set; }
    public PositionConfig Positions { get; set; } = new();
}

public class MapConfig : INameId
{
    public string Id { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int MaxTeamCost { get; set; }
    
    public int PlayerCount { get; set; } = 2;
    public List<TerrainConfig> Terrain { get; set; }
    public List<PositionConfig> PlayerStartPositions { get; set; }
    public List<EntityStartConfig> EntityStartPositions { get; set; }
}