using System.Collections.Generic;
using System.Linq;


public abstract class AbilityTargeting
{
    public GridSelection EffectArea { get; init; } = null;
    public int Targets { get; init; } = 1;
    
    public bool GetEffectSteps(GridSource source, GridPosition target, out GridSteps gridSteps)
    {
        var gridPositions = new List<GridPosition>{target};
        return GetEffectSteps(source, gridPositions, out gridSteps);
    }
    
    // public bool GetEffectSteps(QueryContext ctx, int[] targets, out GridSteps gridSteps)
    // {
    //     var gridPositions = new GridPosition[targets.Length];
    //     for (var i = 0; i < targets.Length; i++) gridPositions[i] = new GridPosition(ctx.Grid, targets[i]);
    //     return GetEffectSteps(ctx, gridPositions, out gridSteps);
    // }
    
    public virtual bool GetPickSteps(GridSource source, GridPosition target, out GridSteps steps)
        => GetEffectSteps(source, target, out steps);

    public abstract GridSteps GetSelectableSteps(GridSource source);
    public abstract bool GetEffectSteps(GridSource source, List<GridPosition> targets, out GridSteps gridSteps);
}

public class FillTargeting : AbilityTargeting
{
    public override GridSteps GetSelectableSteps(GridSource source) => EffectArea.GetGridSteps(source);
    
    public override bool GetEffectSteps(GridSource source, List<GridPosition> targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(source);
        if (targets.Count == 0 || !selectableSteps.Contains(targets[0])) return false;
        steps = EffectArea.GetGridSteps(source);
        return true;
    }
}

public class PositionTargeting : AbilityTargeting
{
    public GridSelection SelectableArea { get; init; }
    public DirectionTargeting Chain { get; init; } = null;
    public bool Distinct { get; init; } = true;

    //public GridSteps GetSelectablePositions(QueryContext ctx) => SelectableArea.GetPositions(ctx);
    
    public override GridSteps GetSelectableSteps(GridSource source) => SelectableArea.GetGridSteps(source);
    
    public override bool GetEffectSteps(GridSource source, List<GridPosition> targets, out GridSteps steps)
    {
        steps = new GridSteps();
        if (targets.Count != Targets) return false;
        if (Distinct && targets.Distinct().Count() != targets.Count) return false;
        var selectableSteps = GetSelectableSteps(source);
        for (var i = 0; i < Targets; i++)
        {
            if (!selectableSteps.Contains(targets[i])) return false;
            AddPickArea(steps, source, targets[i], i);
        }
        return true;
    }

    public override bool GetPickSteps(GridSource source, GridPosition target, out GridSteps steps)
    {
        steps = new GridSteps();
        if (!GetSelectableSteps(source).Contains(target)) return false;
        AddPickArea(steps, source, target, 0);
        return true;
    }

    private void AddPickArea(GridSteps steps, GridSource source, GridPosition target, int index)
    {
        if (EffectArea == null) steps.Add(new GridStep(target, GridDirection.All, 0), index);
        else steps.Add(EffectArea.GetGridSteps(new GridSource(source.Grid, target, source.Entity)), index);
    }
}

public class DirectionTargeting : AbilityTargeting
{
    public int[] Grouping { get; init; }
    public int MaxHits { get; init; } = 0;

    public override GridSteps GetSelectableSteps(GridSource source)
    {
        var grouping = AbsoluteGrouping(source, EffectArea.GetGridSteps(source));
        var mask = new bool[GridTraversal.UnidirectionalCount];
        for (var i = 0; i < mask.Length; i++) mask[i] = grouping[i] != 0;
        var selection = new GridSelection()
        {
            MinDistance = 1,
            Traversals = new List<GridTraversal>()
            {
                new GridTraversal()
                {
                    Direction = GridDirection.Custom,
                    CustomDirection = mask,
                    AbsoluteDirection = true,
                    MaxDistance = 1,
                    Passthrough = Occupant.Any
                }
            }
        };
        return selection.GetGridSteps(source);
    }
    
    public override bool GetEffectSteps(GridSource source, List<GridPosition> targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(source);
        
        if (targets.Count != 1 || !selectableSteps.Contains(targets[0])) return false;
        var targetSteps = selectableSteps.GetStepsAtPosition(targets[0]);
        if (targetSteps.Count > 1) return false;
        
        var effectSteps = EffectArea.GetGridSteps(source);
        if (effectSteps.GroupCount == 1)
        {
            var groupMapSingle = effectSteps.GetGroupMapSingle(AbsoluteGrouping(source, effectSteps));
            foreach (var targetStep in targetSteps) steps.Add(LimitHits(source.Grid, groupMapSingle[targetStep.Direction]));
        }
        else if (effectSteps.GroupCount > 1)
        {
            var unrotate = Opposite(FacingOf(source));
            var relativeGrouping = Grouping ?? Rotated(UngroupedDirections(effectSteps), unrotate);
            var groupMap = effectSteps.GetGroupMap(relativeGrouping);
            foreach (var targetStep in targetSteps)
            {
                var relativeDirection = GridTraversal.Rotate(targetStep.Direction, unrotate);
                steps.Add(LimitHits(source.Grid, groupMap[relativeDirection]));
            }
        }

        return true;
    }

    private static GridDirectionFacing FacingOf(GridSource source) => source.Entity?.Facing ?? GridDirectionFacing.North;

    private static GridDirectionFacing Opposite(GridDirectionFacing facing) => (GridDirectionFacing)((4 - (int)facing) % 4);

    private int[] AbsoluteGrouping(GridSource source, GridSteps effectSteps)
    {
        if (Grouping == null) return UngroupedDirections(effectSteps);
        return Rotated(Grouping, FacingOf(source));
    }

    private static int[] Rotated(int[] grouping, GridDirectionFacing facing)
    {
        var rotated = new int[grouping.Length];
        for (var i = 0; i < grouping.Length; i++) rotated[(int)GridTraversal.Rotate((GridDirection)i, facing)] = grouping[i];
        return rotated;
    }

    private static int[] UngroupedDirections(GridSteps effectSteps)
    {
        var directionMap = effectSteps.GetDirectionMap();
        var grouping = new int[GridTraversal.UnidirectionalCount];
        for (var i = 0; i < grouping.Length; i++) grouping[i] = directionMap.ContainsKey((GridDirection)i) ? i + 1 : 0;
        return grouping;
    }

    private HashSet<GridStep> LimitHits(GridState grid, HashSet<GridStep> steps)
    {
        if (MaxHits <= 0) return steps;
        var limited = new HashSet<GridStep>();
        foreach (var ray in steps.GroupBy(step => step.Direction))
        {
            var hits = 0;
            foreach (var step in ray.OrderBy(step => step.Distance))
            {
                limited.Add(step);
                if (grid.GetEntity(step.Position) != null && ++hits >= MaxHits) break;
            }
        }
        return limited;
    }

    // public override bool GetEffectSteps(QueryContext ctx, (int, int)[] targets, out GridSteps steps)
    // {
    //     steps = new GridSteps();
    //     var (positions, groups) = GetSelectablePositionGroups(ctx);
    //     var selectedGroups = new List<int>();
    //     for (var i = 0; i < Targets; i++)
    //     {
    //         var position = targets[i];
    //         if (!positions.TryGetValue(position, out var group)) return false;
    //
    //         if (group >= GridTraversal.UnidirectionalCount) group = 0;
    //         if (selectedGroups.Contains(group)) return false;
    //         selectedGroups.Add(group);
    //     }
    //     var effectSteps = EffectArea.GetSteps(ctx);
    //     foreach (var group in selectedGroups)
    //     {
    //         var gridSteps = new HashSet<GridStep>();
    //
    //         var gridDirectionSteps = GridSteps.SortByDirection(gridSteps);
    //         var directionSteps = groups[group];
    //         foreach (var step in directionSteps) gridSteps.UnionWith(gridDirectionSteps[step.Direction]);
    //
    //         steps.Add(gridSteps);
    //     }
    //     return true;
    // }
}
