using System.Collections.Generic;
using System.Linq;

public class GridFactory
{
    public Grid2D Create(MapConfig config)
    {
        var terrain = GetTypePositions(config.Terrain);
        var playerStartPositions = GetTypePositions(config.PlayerStartPositions);
        var entityStartPositions = GetTypePositions(config.EntityStartPositions);
        
        return new Grid2D(config.Id, config.X, config.Y, config.MaxTeamCost, config.PlayerCount, terrain, playerStartPositions, entityStartPositions);
    }
    
    private (string[], int[][]) GetTypePositions(List<PositionRangeConfig> configs)
    {
        var positions = new List<List<int>>();
        var types = new List<string>();
        foreach (var config in configs)
        {
            types.Add(config.Type);
            var list = new List<int>();
            list.AddRange(config.Positions);
            foreach (var range in config.Ranges)
            {
                list.AddRange(Enumerable.Range(range.Start, range.End - range.Start + 1));
            }
            positions.Add(list.Distinct().ToList());
        }
        return (types.ToArray(), positions.Select(x => x.ToArray()).ToArray());
    }
}