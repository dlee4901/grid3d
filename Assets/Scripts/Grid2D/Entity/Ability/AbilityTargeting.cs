using System.Collections.Generic;
using System.Linq;


public abstract class AbilityTargeting
{
    public int Targets { get; set; } = 1;
    public GridSelection EffectArea { get; set; } = null;

    //public abstract HashSet<GridStep> GetSelectableSteps(QueryContext ctx);
    public abstract QueryContext TryTargeting(QueryContext ctx, int[] targets, out List<HashSet<GridStep>> steps);
}

public class PositionTargeting : AbilityTargeting
{
    public GridSelection SelectableArea { get; set; }
    public DirectionTargeting Chain { get; set; } = null;

    public HashSet<(int, int)> GetSelectablePositions(QueryContext ctx) => SelectableArea.GetPositions(ctx);

    public HashSet<GridStep> GetSelectableSteps(QueryContext ctx) => SelectableArea.GetSteps(ctx);

    public override QueryContext TryTargeting(QueryContext ctx, int[] targets, out List<HashSet<GridStep>> steps)
    {
        steps = new List<HashSet<GridStep>>();
        var selectablePositions = GetSelectablePositions(ctx);
        for (var i = 0; i < Targets; i++)
        {
            if (i >= Targets) return ctx;

            var position = ctx.Grid.ToPosition2D(targets[i]);
            if (!selectablePositions.Contains(position)) return ctx;

            var newCtx = new QueryContext(ctx.Grid, position, ctx.SourceEntity);
            if (Chain != null) return newCtx;

            steps.Add(EffectArea == null
                ? new HashSet<GridStep> { new GridStep(position, 0, DirectionType.Line) }
                : EffectArea.GetSteps(newCtx));
        }
        return ctx;
    }
}

public class DirectionTargeting : AbilityTargeting
{
    public int[] Grouping { get; set; } = new int[GridTraversal.Directions];

    public (Dictionary<(int, int), int> positions, Dictionary<int, List<GridStep>> groups) GetSelectablePositionGroups(QueryContext ctx)
    {
        var positions = new Dictionary<(int, int), int>();
        var groups = new Dictionary<int, List<GridStep>>();
        var selection = new GridSelection()
        {
            Traversals = new List<GridTraversal>()
            {
                new GridTraversal()
                {
                    Direction = DirectionType.Line,
                    MaxDistance = 1,
                    Passthrough = EntityPassthrough.All
                }
            }
        };
        var steps = selection.GetSteps(ctx);
        foreach (var step in steps)
        {
            var direction = (int)step.Direction;
            if (!groups.TryGetValue(Grouping[direction], out var list))
            {
                list = new List<GridStep>();
                groups.Add(Grouping[direction], list);
            }
            list.Add(step);
            positions.Add(step.Position, Grouping[direction]);
        }

        return (positions, groups);
    }

    public override QueryContext TryTargeting(QueryContext ctx, int[] targets, out List<HashSet<GridStep>> steps)
    {
        steps = new List<HashSet<GridStep>>();
        var (positions, groups) = GetSelectablePositionGroups(ctx);
        var selectedGroups = new List<int>();
        for (var i = 0; i < Targets; i++)
        {
            if (i >= Targets) return ctx;

            var position = ctx.Grid.ToPosition2D(targets[i]);
            if (!positions.TryGetValue(position, out var group)) return ctx;

            if (group >= GridTraversal.Directions) group = 0;
            if (selectedGroups.Contains(group)) return ctx;
            selectedGroups.Add(group);
        }
        var effectSteps = EffectArea.GetSteps(ctx);
        foreach (var group in selectedGroups)
        {
            var gridSteps = new HashSet<GridStep>();

            var gridDirectionSteps = GridSteps.SortByDirection(gridSteps);
            var directionSteps = groups[group];
            foreach (var step in directionSteps) gridSteps.UnionWith(gridDirectionSteps[step.Direction]);

            steps.Add(gridSteps);
        }
        return ctx;
    }
}
