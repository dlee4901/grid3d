using System.Collections.Generic;

public static partial class MapConfigs
{
    public static readonly GridDefinition Map8x8 = new()
    {
        Id = "8x8", X = 8, Y = 8, MaxTeamCost = 22,
        PlayerStartPositions = new List<PositionConfig>
        {
            new() { PositionRanges = new List<IntRange> { new IntRange(0, 15) } },
            new() { PositionRanges = new List<IntRange> { new IntRange(48, 63) } }
        }
    };
}
