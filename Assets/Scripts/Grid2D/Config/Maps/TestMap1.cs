using System.Collections.Generic;

public static partial class MapConfigs
{
    public static readonly GridDefinition TestMap1 = new()
    {
        Id = "TestMap1", X = 8, Y = 8, MaxTeamCost = 22,
        Terrain = new List<TerrainConfig>
        {
            new() { TerrainType = "Void", PositionValues = new List<int> { 26, 37 } }
        },
        PlayerStartPositions = new List<PositionConfig>
        {
            new() { PositionRanges = new List<IntRange> { new IntRange(0, 15) } },
            new() { PositionRanges = new List<IntRange> { new IntRange(48, 63) } }
        },
        EntityStartPositions = new List<EntityStartConfig>
        {
            //new() { EntityId = "ExplosiveBarrel", PositionValues = new() { 42, 44 }, PositionRanges = new() { new IntRange(35, 37) } }
        }
    };
}
