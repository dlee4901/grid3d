using System;
using System.Collections.Generic;

public struct GridStep : IEquatable<GridStep>
{
    public (int x, int y) Position;
    public int Distance;
    public DirectionType Direction;

    public GridStep((int x, int y) position, int distance, DirectionType direction)
    {
        Position = position;
        Distance = distance;
        Direction = direction;
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

public static class GridSteps
{
    public static Dictionary<DirectionType, List<GridStep>> SortByDirection(List<GridStep> steps)
    {
        var dict = new Dictionary<DirectionType, List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Direction, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Direction, list);
            }
            list.Add(step);
        }
        return dict;
    }
    
    public static Dictionary<DirectionType, List<GridStep>> SortByDirection(HashSet<GridStep> steps)
    {
        var dict = new Dictionary<DirectionType, List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Direction, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Direction, list);
            }
            list.Add(step);
        }
        return dict;
    }
    
    public static Dictionary<int, List<GridStep>> SortByDistance(List<GridStep> steps)
    {
        var dict = new Dictionary<int, List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Distance, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Distance, list);
            }
            list.Add(step);
        }
        return dict;
    }
    
    public static Dictionary<int, List<GridStep>> SortByDistance(HashSet<GridStep> steps)
    {
        var dict = new Dictionary<int, List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Distance, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Distance, list);
            }
            list.Add(step);
        }
        return dict;
    }
    
    public static Dictionary<(int, int), List<GridStep>> SortByPosition(List<GridStep> steps)
    {
        var dict = new Dictionary<(int, int), List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Position, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Position, list);
            }
            list.Add(step);
        }
        return dict;
    }
    
    public static Dictionary<(int, int), List<GridStep>> SortByPosition(HashSet<GridStep> steps)
    {
        var dict = new Dictionary<(int, int), List<GridStep>>();
        foreach (var step in steps)
        {
            if (!dict.TryGetValue(step.Position, out var list))
            {
                list = new List<GridStep>();
                dict.Add(step.Position, list);
            }
            list.Add(step);
        }
        return dict;
    }
}