using System.Collections.Generic;

public static partial class MapConfigs
{
    public static readonly GridDefinition Map10x10 = new()
    {
        Id = "10x10", X = 10, Y = 10, MaxTeamCost = 28,
        PlayerStartPositions = new List<PositionConfig>
        {
            new() { PositionRanges = new List<IntRange> { new IntRange(0, 19) } },
            new() { PositionRanges = new List<IntRange> { new IntRange(80, 99) } }
        }
    };
}
