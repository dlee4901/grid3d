using System;
using System.Collections.Generic;
using System.Linq;

public struct GridStep : IEquatable<GridStep>
{
    public GridPosition Position;
    public GridDirection Direction;
    public int Distance;

    public GridStep(GridPosition position, GridDirection direction, int distance)
    {
        Position = position;
        Direction = direction;
        Distance = distance;
    }

    public bool Equals(GridStep other)
    {
        return Position.Equals(other.Position) && Direction == other.Direction; //&& Distance == other.Distance;
    }

    public override bool Equals(object? obj)
    {
        return obj is GridStep other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Position, (int)Direction);
    }
}

public class GridSteps
{
    private readonly List<HashSet<GridStep>> _steps = new() { new HashSet<GridStep>() };
    private readonly List<Dictionary<GridPosition, HashSet<GridStep>>> _positions = new() { new Dictionary<GridPosition, HashSet<GridStep>>() };

    public void Add(GridStep gridStep, int index=0)
    {
        ExpandListCount(index);
        if (!_steps[index].Add(gridStep)) return;
        if (!_positions[index].TryGetValue(gridStep.Position, out var steps))
        {
            steps = new HashSet<GridStep>();
            _positions[index].Add(gridStep.Position, steps);
        }
        steps.Add(gridStep);
    }
    
    public void Add(HashSet<GridStep> gridSteps, int index=0)
    {
        foreach (var step in gridSteps) Add(step, index);
    }
    
    public void Add(GridSteps gridSteps, int index=0)
    {
        foreach (var steps in gridSteps._steps)
        foreach (var step in steps) Add(step, index);
    }
    
    public bool Contains(GridStep gridStep, int index=0)
    {
        return index < _steps.Count && _steps[index].Contains(gridStep);
    }
    
    public bool Contains(GridPosition position, int index=0)
    {
        return index < _positions.Count && _positions[index].ContainsKey(position);
    }
    
    public HashSet<GridStep> GetStepsAtPosition(GridPosition position, int index=0)
    {
        return _positions[index][position];
    }
    
    public HashSet<GridStep> GetSteps(int index=0)
    {
        return _steps[index];
    }
    
    public HashSet<GridPosition> GetPositions(int index=0)
    {
        return _positions[index].Keys.ToHashSet();
    }
    
    public Dictionary<GridDirection, HashSet<GridStep>> GetDirectionMap(int index=0)
    {
        var map = new Dictionary<GridDirection, HashSet<GridStep>>();
        if (index >= _steps.Count) return map;
        foreach (var step in _steps[index])
        {
            if (!map.TryGetValue(step.Direction, out var hashset))
            {
                hashset = new HashSet<GridStep>();
                map.Add(step.Direction, hashset);
            }
            hashset.Add(step);
        }
        return map;
    }
    
    public Dictionary<int, HashSet<GridStep>> GetDistanceMap(int index=0)
    {
        var map = new Dictionary<int, HashSet<GridStep>>();
        if (index >= _steps.Count) return map;
        foreach (var step in _steps[index])
        {
            if (!map.TryGetValue(step.Distance, out var hashset))
            {
                hashset = new HashSet<GridStep>();
                map.Add(step.Distance, hashset);
            }
            hashset.Add(step);
        }
        return map;
    }
    
    public Dictionary<GridPosition, HashSet<GridStep>> GetPositionMap(int index=0)
    {
        var map = new Dictionary<GridPosition, HashSet<GridStep>>();
        if (index >= _steps.Count) return map;
        foreach (var step in _steps[index])
        {
            if (!map.TryGetValue(step.Position, out var hashset))
            {
                hashset = new HashSet<GridStep>();
                map.Add(step.Position, hashset);
            }
            hashset.Add(step);
        }
        return map;
    }
    
    public Dictionary<GridDirection, HashSet<GridStep>> GetGroupMap(int[] groups, int index=0)
    {
        var map = new Dictionary<GridDirection, HashSet<GridStep>>();
        if (index >= _steps.Count || groups.Length != GridTraversal.UnidirectionalCount) return map;
        var directionMap = GetDirectionMap(index);
        var groupNumToSteps = new Dictionary<int, HashSet<GridStep>>();
        for (var i = 0; i < GridTraversal.UnidirectionalCount; i++)
        {
            if (!groupNumToSteps.TryGetValue(groups[i], out var steps))
            {
                steps = new HashSet<GridStep>();
                groupNumToSteps.Add(groups[i], steps);
            }
            if (directionMap.TryGetValue((GridDirection)i, out var directionSteps)) 
                steps.UnionWith(directionSteps);
        }
        for (var i = 0; i < GridTraversal.UnidirectionalCount; i++) map[(GridDirection)i] = groupNumToSteps[groups[i]];
        return map;
    }
    
    private void ExpandListCount(int index)
    {
        if (index >= _steps.Count) for (var i = _steps.Count; i <= index; i++) _steps.Add(new HashSet<GridStep>());
        if (index >= _positions.Count) for (var i = _positions.Count; i <= index; i++) _positions.Add(new Dictionary<GridPosition, HashSet<GridStep>>());
    }
}