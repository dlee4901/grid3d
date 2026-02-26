using System;
using System.Collections.Generic;
using System.Linq;

public class GridSelection
{
    public List<GridTraversal> Traversals { get; set; }
    
    public List<IntRange> ExcludedDistanceRanges { get; set; } = new();
    public RangePattern? RangePattern { get; set; }
    
    public PredicateConfig EntityAllowlist { get; set; }
    public PredicateConfig EntityDenylist { get; set; }
    
    public List<Step> GetSteps(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        var steps = new HashSet<Step>();
        foreach (var traversal in Traversals)
            steps.UnionWith(traversal.GetStepsSet(grid, startPosition, sourceEntity));
        return steps.ToList();
    }
    
    public void AddExcludedDistanceRangePattern()
    {
        if (RangePattern == null || RangePattern?.Span == 0 || RangePattern?.Gap == 0) return;
        var maxDistance = Traversals.Select(traversals => traversals.MaxDistance).Prepend(0).Max();
        for (var i = RangePattern?.Start; i <= maxDistance; i += RangePattern?.Span + RangePattern?.Gap + 1)
        {
            ExcludedDistanceRanges.Add((i.Value, i + RangePattern?.Span - 1));
        }
    }
    
    public Dictionary<int, int> GetTileDistances(Grid2D grid, int startPosition, Entity? sourceEntity=null)
    {
        var tileSelection = new TileSelection(grid);
        foreach (var tileSelector in TileSelectors)
        {
            var selection = tileSelector.GetTileSelection(grid, startPosition, sourceEntity);
            tileSelection.Merge(selection);
        }
        if (MinDistance > 0) ExcludedDistanceRanges.Add((0, MinDistance));
        
        return GetTileDistancesNotInRanges(ExcludedDistanceRanges, false, true, MaxDistance, EntityAllowlist, EntityDenylist);
    }
    
    public Dictionary<int, int> GetTileDistancesNotInRanges(List<(int, int)> ranges, bool includeLower=true, bool includeUpper=true, int maxDistance=0, PredicateConfig entityAllowList=null, PredicateConfig entityDenyList=null)
    {
        var result = new Dictionary<int, int>();
        foreach (var (tile, distance) in _tileDistances)
        {
            if (distance == -1) continue;
            if (maxDistance > 0 && distance > maxDistance) continue;
            foreach (var (min, max) in ranges)
            {
                if ((includeLower ? distance > min : distance >= min) &&
                    (includeUpper ? distance < max : distance <= max)) continue;
                // if (excludeQuery != null)
                // {
                //     var entity = _grid.GetEntity(tile);
                //     var query = excludeQuery.Build();
                //     if (entity != null && query(entity))
                //     {
                //         continue;
                //     }
                // }
                Entity entity;
                if (entityAllowList != null && (entity = _grid.GetEntity(tile)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(entityAllowList);
                    if (!predicate(entity)) continue;
                }
                if (entityDenyList != null && (entity = _grid.GetEntity(tile)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(entityAllowList);
                    if (predicate(entity)) continue;
                }
                result[_grid.ToPosition1D(tile)] = distance;
                break;
            }
        }
        return result;
    }
}