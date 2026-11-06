using System;
using System.Collections.Generic;
using System.Linq;

public class GridSelection
{
    public List<GridTraversal> Traversals { get; init; }
    public int MinDistance { get; init; } = 0;
    public int MaxDistance { get; init; } = 0;
    public List<IntRange> ExcludedDistanceRanges { get; init; } = new();
    public IntRangePattern ExcludedDistRangePattern { get; init; }

    public Occupant Selectable { get; init; } = Occupant.Any;

    public GridSteps GetGridSteps(GridSource source)
    {
        var gridSteps = new GridSteps();
        for (var i = 0; i < Traversals.Count; i++)
        {
            var traversal = Traversals[i];
            var steps = traversal.GetSteps(source);
            var maxDistance = MaxDistance;
            if (MaxDistance <= 0) maxDistance = Math.Max(maxDistance, traversal.MaxDistance);
            var excludedDistances = GetExcludedDistances(maxDistance);
            
            foreach (var step in steps)
            {
                if (step.Distance < MinDistance || step.Distance > maxDistance || excludedDistances.Contains(step.Distance) || gridSteps.Contains(step)) continue;
                if ((Selectable & Occupants.At(source.Grid, step.Position, source.Entity)) == 0) continue;
                gridSteps.Add(step, i);
            }
        }
        return gridSteps;
    }

    private HashSet<int> GetExcludedDistances(int maxDistance)
    {
        var excludedDistances = new HashSet<int>();
        var intRanges = new List<IntRange>();
        intRanges.AddRange(ExcludedDistanceRanges);
        if (ExcludedDistRangePattern != null)
            intRanges.AddRange(ExcludedDistRangePattern.GetIntRanges(maxDistance));
        foreach (var range in intRanges)
            excludedDistances.UnionWith(range.GetValues());
        return excludedDistances;
    }
}
