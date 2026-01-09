#nullable enable
using System.Collections.Generic;

public struct RangePattern
{
    public int Start { get; set; }
    public int Span { get; set; }
    public int Gap { get; set; }
}

public class TileSelectionBuilder
{
    public List<TileSelector> TileSelectors { get; set; } = new();
    public int MinDistance { get; set; } = 0;
    public int MaxDistance { get; set; } = 0;
    public List<(int, int)> ExcludedDistanceRanges { get; set; } = new();
    public RangePattern? RangePattern { get; set; }
    public QueryNode? EntityAllowlist { get; set; }
    public QueryNode? EntityDenylist { get; set; }
    
    // public TileSelectionBuilder(TileSelector tileSelector, int minDistance=0, int maxDistance=0, List<(int, int)> excludedDistanceRanges=null) : this(new List<TileSelector>{tileSelector}, minDistance, maxDistance, excludedDistanceRanges) {}
    //
    // public TileSelectionBuilder(List<TileSelector> tileSelectors=null, int minDistance=0, int maxDistance=0, List<(int, int)> excludedDistanceRanges=null)
    // {
    //     TileSelectors = tileSelectors ?? new List<TileSelector>();
    //     MinDistance = minDistance;
    //     MaxDistance = maxDistance;
    //     ExcludedDistanceRanges = excludedDistanceRanges ?? new List<(int, int)>();
    // }
    
    public void SetDistances(int minDistance, int maxDistance)
    {
        MinDistance = minDistance;
        MaxDistance = maxDistance;
    }
    
    public void AddSelector(TileSelector tileSelector)
    {
        TileSelectors.Add(tileSelector);
    }
    
    public void AddExcludedDistanceRange((int, int) range)
    {
        ExcludedDistanceRanges.Add(range);
    }
    
    public void AddExcludedDistanceRanges(List<(int, int)> ranges)
    {
        ExcludedDistanceRanges.AddRange(ranges);
    }
    
    public void AddExcludedDistanceRangePattern(RangePattern rangePattern)
    {
        for (var i = rangePattern.Start; i <= MaxDistance; i += rangePattern.Span + rangePattern.Gap + 1)
        {
            ExcludedDistanceRanges.Add((i, i + rangePattern.Span));
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
        
        return tileSelection.GetTileDistancesNotInRanges(ExcludedDistanceRanges, false, true, MaxDistance, excludeQuery);
    }
}