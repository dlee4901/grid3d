using System.Collections.Generic;

public static partial class MapConfigs
{
    public static readonly GridDefinition TestMap1 = new()
    {
        Id = "TestMap1", X = 11, Y = 11, MaxTeamCost = 22,
        Terrain = new List<TerrainConfig>
        {
            new() { TerrainType = "Void", PositionValues = new() { 60, 62 } }
        },
        PlayerStartPositions = new List<PositionConfig>
        {
            new() { PositionRanges = new() { new IntRange(0, 22) } },
            new() { PositionRanges = new() { new IntRange(97, 120) } }
        },
        EntityStartPositions = new List<EntityStartConfig>
        {
            //new() { EntityId = "ExplosiveBarrel", PositionValues = new() { 42, 44 }, PositionRanges = new() { new IntRange(35, 37) } }
        }
    };
}
