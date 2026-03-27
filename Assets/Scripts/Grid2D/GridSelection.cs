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
    
    public HashSet<(int, int)> GetSelection(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null, bool filterEntities=true)
    {
        var selection = new HashSet<(int, int)>();
        var (steps, maxDistance) = GetUnfilteredSteps(grid, startPosition, sourceEntity);
        var excludedDistances = GetExcludedDistances(maxDistance);
        foreach (var step in steps)
        {
            if (step.Distance < MinDistance || step.Distance > maxDistance || excludedDistances.Contains(step.Distance) || selection.Contains(step.Position)) continue;
            if (filterEntities)
            {
                Entity entity;
                if (EntityAllowlist != null && (entity = grid.GetEntity(step.Position)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(EntityAllowlist);
                    if (!predicate(entity)) continue;
                }
                if (EntityDenylist != null && (entity = grid.GetEntity(step.Position)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(EntityDenylist);
                    if (predicate(entity)) continue;
                }
            }
            selection.Add(step.Position);
        }
        return selection;
    }
    
    public HashSet<Step> GetSteps(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null, bool filterEntities=true)
    {
        var filteredSteps = new HashSet<Step>();
        var (steps, maxDistance) = GetUnfilteredSteps(grid, startPosition, sourceEntity);
        var excludedDistances = GetExcludedDistances(maxDistance);
        foreach (var step in steps)
        {
            if (step.Distance < MinDistance || step.Distance > maxDistance || excludedDistances.Contains(step.Distance) || filteredSteps.Contains(step)) continue;
            if (filterEntities)
            {
                Entity entity;
                if (EntityAllowlist != null && (entity = grid.GetEntity(step.Position)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(EntityAllowlist);
                    if (!predicate(entity)) continue;
                }
                if (EntityDenylist != null && (entity = grid.GetEntity(step.Position)) != null)
                {
                    var predicate = PredicateFactory<Entity>.Create(EntityDenylist);
                    if (predicate(entity)) continue;
                }
            }
            filteredSteps.Add(step);
        }
        return filteredSteps;
    }
    
    private (HashSet<Step> steps, int maxDistance) GetUnfilteredSteps(Grid2D grid, (int, int) startPosition, Entity? sourceEntity=null)
    {
        var steps = new HashSet<Step>();
        var maxDistance = MaxDistance;
        foreach (var traversal in Traversals)
        {
            steps.UnionWith(traversal.GetStepsSet(grid, startPosition, sourceEntity));
            if (MaxDistance <= 0) maxDistance = Math.Max(maxDistance, traversal.MaxDistance); 
        }
        return (steps, maxDistance);
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