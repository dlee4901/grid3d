using System.Collections.Generic;

public static partial class MapConfigs
{
    public static readonly GridDefinition Map6x6 = new()
    {
        Id = "6x6", X = 6, Y = 6, MaxTeamCost = 16,
        PlayerStartPositions = new List<PositionConfig>
        {
            new() { PositionRanges = new List<IntRange> { new IntRange(0, 11) } },
            new() { PositionRanges = new List<IntRange> { new IntRange(24, 35) } }
        }
    };
}
