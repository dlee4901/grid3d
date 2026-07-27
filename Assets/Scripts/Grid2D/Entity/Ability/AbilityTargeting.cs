using System.Collections.Generic;
using System.Linq;

public abstract class AbilityTargeting
{
    public List<GridSelection> SelectableAreas { get; set; }
    public int SelectionAmount { get; set; } = 1;

    public HashSet<(int, int)> GetRange(QueryContext ctx)
    {
        var selectableAreasCombined = new HashSet<(int, int)>();
        foreach (var gridSelection in SelectableAreas)
            foreach (var step in gridSelection.GetSteps(ctx, false))
                selectableAreasCombined.Add(step.Position);
        return selectableAreasCombined;
    }

    public (List<(int, int)> areas, List<int> splits) GetSelectablePositions(QueryContext ctx)
    {
        var areas = new List<(int, int)>();
        var splits = new List<int>();
        foreach (var gridSelection in SelectableAreas)
        {
            var area = gridSelection.GetPositions(ctx).ToList();
            splits.Add(area.Count);
            areas.AddRange(area);
        }
        return (areas, splits);
    }
    
    // public (List<Step> areas, List<int> splits) GetSteps(QueryContext ctx)
    // {
    //     
    // }

    public abstract bool TryGetEffectPositions(QueryContext ctx, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null, (List<(int, int)> areas, List<int> splits)? selectable=null);
}

public class SingleAbilityTargeting : AbilityTargeting
{
    public override bool TryGetEffectPositions(QueryContext ctx, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null, (List<(int, int)> areas, List<int> splits)? selectable=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        var (areas, splits) = selectable ?? GetSelectablePositions(ctx);
        foreach (var position in selectedPositions)
        {
            if (!areas.Contains(position))
                return false;
            effectPositions.Add(position);
        }
        return true;
    }
}

public class AreaAbilityTargeting : AbilityTargeting
{
    public GridSelection EffectArea { get; set; }

    public override bool TryGetEffectPositions(QueryContext ctx, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null, (List<(int, int)> areas, List<int> splits)? selectable=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        var (areas, splits) = selectable ?? GetSelectablePositions(ctx);
        foreach (var position in selectedPositions)
        {
            if (!areas.Contains(position))
                return false;
            effectPositions.UnionWith(EffectArea.GetPositions(new QueryContext(ctx.Grid, position, ctx.SourceEntity)));
        }
        return true;
    }
}

public class FillAbilityTargeting : AbilityTargeting
{
    public bool CombineAreas { get; set; } = false;

    public override bool TryGetEffectPositions(QueryContext ctx, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null, (List<(int, int)> areas, List<int> splits)? selectable=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        if (!CombineAreas && (splitAreaSelections == null || splitAreaSelections.Count != SelectionAmount))
            return false;

        var (areas, splits) = selectable ?? GetSelectablePositions(ctx);
        if (CombineAreas)
        {
            if (!areas.Contains(selectedPositions[0]))
                return false;
            effectPositions.UnionWith(areas);
            return true;
        }

        var splitAreas = new List<List<(int, int)>>();
        var index = 0;
        foreach (var split in splits)
        {
            var range = areas.GetRange(index, split);
            splitAreas.Add(range);
            index += split;
        }
        for (var i = 0; i < selectedPositions.Count; i++)
        {
            var areaIndex = splitAreaSelections[i];
            if (!splitAreas[areaIndex].Contains(selectedPositions[i]))
                return false;
            effectPositions.UnionWith(splitAreas[i]);
        }
        return true;
    }
}

// public class ProjectileSelection : AbilitySelection
// {
//     public int ProjectileAmount { get; set; } = 1;
//     public bool SplitSelectionDirections { get; set; } = false;
// }
