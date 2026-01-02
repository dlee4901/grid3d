using System.Collections.Generic;
using System.Linq;

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
    private List<TileSelector> _tileSelectors;
    
    private readonly Grid2D _grid;
    private readonly int _startPosition;
    private readonly Entity _sourceEntity;
    
    private readonly int _minDistance;
    private readonly int _maxDistance;
    private readonly RangePattern? _rangePattern;
    
    private readonly List<(int, int)> _excludedDistanceRanges;
    
    public TileSelectionBuilder(Grid2D grid, int startPosition, Entity sourceEntity=null, int minDistance=0, int maxDistance=0, RangePattern? rangePattern=null)
    {
        _grid = grid;
        _startPosition = startPosition;
        _sourceEntity = sourceEntity;
        _minDistance = minDistance;
        _maxDistance = maxDistance;
        _rangePattern = rangePattern;
    }
    
    public void AddSelector(TileSelector tileSelector)
    {
        _tileSelectors.Add(tileSelector);
    }
    
    public void AddExcludedDistanceRange((int, int) range)
    {
        _excludedDistanceRanges.Add(range);
    }
    
    public void AddExcludedDistanceRanges(List<(int, int)> ranges)
    {
        _excludedDistanceRanges.AddRange(ranges);
    }
    
    public Dictionary<int, int> GetTileDistances(QueryBuilder<Entity> excludeQuery=null)
    {
        var tileSelection = new TileSelection(_grid);
        foreach (var tileSelector in _tileSelectors)
        {
            var selection = tileSelector.GetTileSelection(_grid, _startPosition, _sourceEntity);
            tileSelection.Merge(selection);
        }
        
        if (_minDistance > 0) _excludedDistanceRanges.Add((0, _minDistance));
        if (_rangePattern.HasValue)
        {
            var rangePattern = _rangePattern.Value;
            for (var i = rangePattern.Start; i <= _maxDistance; i += rangePattern.Span + rangePattern.Gap + 1)
            {
                _excludedDistanceRanges.Add((i, i + rangePattern.Span));
            }
        }
        
        return tileSelection.GetTileDistancesNotInRanges(_excludedDistanceRanges, false, true, _maxDistance, excludeQuery);
    }
}