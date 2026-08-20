using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;

public class GridSelection
{
    public List<GridTraversal> Traversals { get; set; }
    public int MinDistance { get; set; } = 0;
    public int MaxDistance { get; set; } = 0;
    public List<IntRange> ExcludedDistanceRanges { get; set; } = new();
    public IntRangePattern ExcludedDistRangePattern { get; set; }

    public PredicateConfig EntityAllowlist { get; set; }
    public PredicateConfig EntityDenylist { get; set; }

    public GridSteps GetGridSteps(GridSource source, bool filterEntities=true)
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
                if (filterEntities)
                {
                    Entity entity;
                    if (EntityAllowlist != null && (entity = source.Grid.GetEntity(step.Position)) != null)
                    {
                        var predicate = PredicateFactory<Entity>.Create(EntityAllowlist);
                        if (!predicate(entity)) continue;
                    }
                    if (EntityDenylist != null && (entity = source.Grid.GetEntity(step.Position)) != null)
                    {
                        var predicate = PredicateFactory<Entity>.Create(EntityDenylist);
                        if (predicate(entity)) continue;
                    }
                }
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
            excludedDistances.AddRange(range.GetValues());
        return excludedDistances;
    }
}
