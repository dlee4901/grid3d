using System.Collections.Generic;

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
