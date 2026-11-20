using System;
using System.Collections.Generic;
using System.Linq;

public class GridSelection
{
    public List<GridTraversal> Traversals { get; init; }
    public int MinDistance { get; init; } = 0;
    public int? MaxDistance { get; init; }
    public List<IntRange> ExcludedDistanceRanges { get; init; } = new();
    public IntRangePattern ExcludedDistRangePattern { get; init; }

    public Occupant Selectable { get; init; } = Occupant.Any;
    public TerrainType[] Terrain { get; init; }

    public GridSteps GetGridSteps(GridSource source)
    {
        var gridSteps = new GridSteps();
        for (var i = 0; i < Traversals.Count; i++)
        {
            var traversal = Traversals[i];
            var steps = traversal.GetSteps(source);
            var excludedDistances = GetExcludedDistances(steps);

            foreach (var step in steps)
            {
                if (step.Distance < MinDistance || step.Distance > MaxDistance || excludedDistances.Contains(step.Distance) || gridSteps.Contains(step)) continue;
                if ((Selectable & Occupants.At(source.Grid, step.Position, source.Entity)) == 0) continue;
                if (!MatchesTerrain(source.Grid, step.Position)) continue;
                gridSteps.Add(step, i);
            }
        }
        return gridSteps;
    }

    private bool MatchesTerrain(GridState grid, GridPosition position)
        => Terrain == null || grid.TryGetTerrain(position, out var terrain) && Array.IndexOf(Terrain, terrain) >= 0;

    private HashSet<int> GetExcludedDistances(List<GridStep> steps)
    {
        var reach = steps.Count == 0 ? 0 : steps.Max(step => step.Distance);
        var excludedDistances = new HashSet<int>();
        var intRanges = new List<IntRange>();
        intRanges.AddRange(ExcludedDistanceRanges);
        if (ExcludedDistRangePattern != null)
            intRanges.AddRange(ExcludedDistRangePattern.GetIntRanges(reach));
        foreach (var range in intRanges)
            excludedDistances.UnionWith(range.GetValues());
        return excludedDistances;
    }
}
