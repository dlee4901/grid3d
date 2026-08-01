using System.Collections.Generic;
using System.Linq;


public abstract class AbilityTargeting
{
    public GridSelection EffectArea { get; set; } = null;
    public int Targets { get; set; } = 1;
    
    public bool GetEffectSteps(QueryContext ctx, int[] targets, out GridSteps gridSteps)
    {
        var targets2D = new (int, int)[targets.Length];
        for (var i = 0; i < targets.Length; i++) targets2D[i] = ctx.Grid.Definition.ToPosition2D(targets[i]);
        return GetEffectSteps(ctx, targets2D, out gridSteps);
    }
    
    public abstract GridSteps GetSelectableSteps(QueryContext ctx);
    public abstract bool GetEffectSteps(QueryContext ctx, (int, int)[] targets, out GridSteps gridSteps);
}

public class FillTargeting : AbilityTargeting
{
    public override GridSteps GetSelectableSteps(QueryContext ctx) => EffectArea.GetSteps(ctx);
    
    public override bool GetEffectSteps(QueryContext ctx, (int, int)[] targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(ctx);
        if (targets.Length == 0 || !selectableSteps.Contains(targets[0])) return false;
        steps = EffectArea.GetSteps(ctx);
        return true;
    }
}

public class PositionTargeting : AbilityTargeting
{
    public GridSelection SelectableArea { get; set; }
    public DirectionTargeting Chain { get; set; } = null;

    //public GridSteps GetSelectablePositions(QueryContext ctx) => SelectableArea.GetPositions(ctx);
    
    public override GridSteps GetSelectableSteps(QueryContext ctx) => SelectableArea.GetSteps(ctx);
    
    public override bool GetEffectSteps(QueryContext ctx, (int, int)[] targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(ctx);
        for (var i = 0; i < Targets; i++)
        {
            if (!selectableSteps.Contains(targets[i])) return false;

            var ctxTarget = new QueryContext(ctx.Grid, targets[i], ctx.SourceEntity);

            if (EffectArea == null)  steps.Add(new GridStep(targets[i], 0, GridDirection.Line), i);
            else steps.Add(EffectArea.GetSteps(ctxTarget), i);
        }
        return true;
    }
}

public class DirectionTargeting : AbilityTargeting
{
    public int[] Grouping { get; set; } = new int[GridTraversal.UnidirectionalCount];

    // public (Dictionary<(int, int), int> positions, Dictionary<int, List<GridStep>> groups) GetSelectablePositionGroups(QueryContext ctx)
    // {
    //     var positions = new Dictionary<(int, int), int>();
    //     var groups = new Dictionary<int, List<GridStep>>();
    //     var steps = GetSelectableSteps(ctx);
    //     foreach (var step in steps)
    //     {
    //         var direction = (int)step.Direction;
    //         if (!groups.TryGetValue(Grouping[direction], out var list))
    //         {
    //             list = new List<GridStep>();
    //             groups.Add(Grouping[direction], list);
    //         }
    //         list.Add(step);
    //         positions.Add(step.Position, Grouping[direction]);
    //     }
    //
    //     return (positions, groups);
    // }
    
    public override GridSteps GetSelectableSteps(QueryContext ctx)
    {
        var selection = new GridSelection()
        {
            Traversals = new List<GridTraversal>()
            {
                new GridTraversal()
                {
                    Direction = GridDirection.Line,
                    MaxDistance = 1,
                    Passthrough = EntityPassthrough.All
                }
            }
        };
        return selection.GetSteps(ctx);
    }
    
    public override bool GetEffectSteps(QueryContext ctx, (int, int)[] targets, out GridSteps steps)
    {
        steps = new GridSteps();
        var selectableSteps = GetSelectableSteps(ctx);
        var effectSteps = EffectArea.GetSteps(ctx);
        var groupMap = effectSteps.GetGroupMap(Grouping);
        
        if (targets.Length == 0 || !selectableSteps.Contains(targets[0])) return false;
        var targetSteps = selectableSteps.GetStepsAtPosition(targets[0]);
        if (targetSteps.Count > 1) return false;
        foreach (var targetStep in targetSteps) steps.Add(groupMap[targetStep.Direction]);
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
