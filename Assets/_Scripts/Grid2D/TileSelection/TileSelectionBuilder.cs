using System.Collections.Generic;

public struct RangePattern
{
    public readonly int Start;
    public readonly int Span;
    public readonly int Gap;

    public RangePattern(int start,  int span, int gap)
    {
        Start = start;
        Span = span;
        Gap = gap;
    }
}

public class TileSelectionBuilder
{
    public List<TileSelector> TileSelectors { get; set; } = new();
    public int MinDistance { get; set; }
    public int MaxDistance { get; set; }
    public List<(int, int)> ExcludedDistanceRanges { get; set; } = new();
    
    public TileSelectionBuilder(TileSelector tileSelector, int minDistance=0, int maxDistance=0, List<(int, int)> excludedDistanceRanges=null) : this(new List<TileSelector>{tileSelector}, minDistance, maxDistance, excludedDistanceRanges) {}
    
    public TileSelectionBuilder(List<TileSelector> tileSelectors=null, int minDistance=0, int maxDistance=0, List<(int, int)> excludedDistanceRanges=null)
    {
        TileSelectors = tileSelectors ?? new List<TileSelector>();
        MinDistance = minDistance;
        MaxDistance = maxDistance;
        ExcludedDistanceRanges = excludedDistanceRanges ?? new List<(int, int)>();
    }
    
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
    
    public Dictionary<int, int> GetTileDistances(Grid2D grid, int startPosition, Entity sourceEntity=null, QueryBuilder<Entity> excludeQuery=null)
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