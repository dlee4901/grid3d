using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public abstract class AbilityTargeting
{
    public GridSelection EffectArea { get; set; } = null;
    public int Targets { get; set; } = 1;
    
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
    public GridSelection SelectableArea { get; set; }
    public DirectionTargeting Chain { get; set; } = null;

    //public GridSteps GetSelectablePositions(QueryContext ctx) => SelectableArea.GetPositions(ctx);
    
    public override GridSteps GetSelectableSteps(GridSource source) => SelectableArea.GetGridSteps(source);
    
    public override bool GetEffectSteps(GridSource source, List<GridPosition> targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(source);
        for (var i = 0; i < Targets; i++)
        {
            if (!selectableSteps.Contains(targets[i])) return false;

            var ctxTarget = new GridSource(source.Grid, targets[i], source.Entity);

            if (EffectArea == null) steps.Add(new GridStep(targets[i], GridDirection.Line, 0), i);
            else steps.Add(EffectArea.GetGridSteps(ctxTarget), i);
        }
        return true;
    }
}

public class DirectionTargeting : AbilityTargeting
{
    public int[] Grouping { get; set; } = new int[GridTraversal.UnidirectionalCount];
    
    public override GridSteps GetSelectableSteps(GridSource source)
    {
        var mask = new bool[GridTraversal.UnidirectionalCount];
        for (var i = 0; i < mask.Length; i++) mask[i] = Grouping[i] != 0;
        var selection = new GridSelection()
        {
            MinDistance = 1,
            Traversals = new List<GridTraversal>()
            {
                new GridTraversal()
                {
                    Direction = GridDirection.Custom,
                    CustomDirection = mask,
                    MaxDistance = 1,
                    Passthrough = EntityPassthrough.All
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
            var groupMapSingle = effectSteps.GetGroupMapSingle(Grouping);
            foreach (var targetStep in targetSteps) steps.Add(groupMapSingle[targetStep.Direction]);
        }
        else if (effectSteps.GroupCount > 1)
        {
            var groupMap = effectSteps.GetGroupMap(Grouping);
            foreach (var targetStep in targetSteps) steps.Add(groupMap[targetStep.Direction]);
        }
        
        return true;
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
