using System;
using System.Collections.Generic;
using System.Linq;

public class TileSelection
{   
    private readonly Dictionary<(int, int), int> _tileDistances;
    private readonly Grid2D _grid;
    
    public TileSelection(Grid2D grid, Dictionary<(int, int), int> tileDistances=null)
    {
        _grid = grid;
        if (tileDistances != null) _tileDistances = tileDistances;
        else
        {
            _tileDistances = new Dictionary<(int, int), int>();
            for (var i = 0; i < grid.GetSize(); i++) _tileDistances[grid.ToPosition2D(i)] = -1;
        }
    }
    
    public void Merge(TileSelection tileSelection)
    {
        foreach (var kvp in tileSelection._tileDistances) UpdateTileDistance(kvp.Key, kvp.Value);
    }
    
    public void UpdateTileDistance((int, int) tile, int distance)
    {
        if (_tileDistances[tile] == -1) _tileDistances[tile] = distance;
        else _tileDistances[tile] = Math.Min(_tileDistances[tile], distance);
    }
    
    public IEnumerable<(int, int)> GetTilesAtDistance(int distance)
    {
        return _tileDistances.Where(kvp => kvp.Value == distance).Select(kvp => kvp.Key);
    }
    
    public IEnumerable<(int, int)> GetTilesInDistanceRange(int lower, int upper, bool includeLower=true, bool includeUpper=true)
    {
        return _tileDistances
            .Where(kvp => 
                (includeLower ? kvp.Value >= lower : kvp.Value > lower) &&
                (includeUpper ? kvp.Value <= upper : kvp.Value < upper))
            .Select(kvp => kvp.Key);
    }
    
    public int GetTileDistance((int, int) tile)
    {
        return _tileDistances[tile];
    }
    
    public Dictionary<int, int> GetTileDistances(bool selectable=true)
    {
        var result = new Dictionary<int, int>();
        foreach (var (tile, distance) in _tileDistances)
        {
            if (selectable && distance == -1) continue;
            var position = _grid.ToPosition1D(tile);
            result.Add(position, distance);
        }
        return result;
    }
    
    public Dictionary<int, int> GetSelectableTileDistances()
    {
        return _tileDistances.Where(kvp => kvp.Value > -1).ToDictionary(kvp => _grid.ToPosition1D(kvp.Key), kvp => kvp.Value);
    }
    
    public Dictionary<int, int> GetTileDistancesInRanges(List<(int, int)> ranges, bool includeLower=true, bool includeUpper=true, int maxDistance=0)
    {
        var result = new Dictionary<int, int>();
        foreach (var (tile, distance) in _tileDistances)
        {
            if (distance == -1) continue;
            if (maxDistance > 0 && distance > maxDistance) continue;
            foreach (var (min, max) in ranges)
            {
                if ((includeLower ? distance >= min : distance > min) && (includeUpper ? distance <= max : distance < max))
                {
                    result[_grid.ToPosition1D(tile)] = distance;
                    break;
                }
            }
        }
        return result;
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