using System.Collections.Generic;

public class PositionConfig
{
    public List<int> PositionValues { get; set; } = new();
    public List<IntRange> PositionRanges { get; set; } = new();
}

public class TerrainConfig : PositionConfig
{
    public string TerrainType { get; set; }
}

public class EntityStartConfig : PositionConfig
{
    public string EntityId { get; set; }
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