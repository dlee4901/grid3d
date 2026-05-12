using System.Collections.Generic;
using System.Linq;

public abstract class SkillSelection
{
    public List<GridSelection> SelectableAreas { get; set; }
    public int SelectionAmount { get; set; } = 1;
    
    public HashSet<(int, int)> GetRange(Grid2D grid, (int, int) startPosition, Entity sourceEntity)
    {
        var selectableAreasCombined = new HashSet<(int, int)>();
        foreach (var gridSelection in SelectableAreas)
            foreach (var step in gridSelection.GetSteps(grid, startPosition, sourceEntity, false))
                selectableAreasCombined.Add(step.Position);
        return selectableAreasCombined;
    }
    
    public (List<(int, int)> areas, List<int> splits) GetSelectablePositions(Grid2D grid, (int, int) startPosition, Entity sourceEntity)
    {
        var areas = new List<(int, int)>();
        var splits = new List<int>();
        foreach (var gridSelection in SelectableAreas)
        {
            var area = gridSelection.GetSelection(grid, startPosition, sourceEntity).ToList();
            splits.Add(area.Count);
            areas.AddRange(area);
        }
        return (areas, splits);
    }
    
    public abstract bool TryGetEffectPositions(Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null);
}

public class SingleSkillSelection : SkillSelection
{
    public override bool TryGetEffectPositions(Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        var (areas, splits) = GetSelectablePositions(grid, startPosition, sourceEntity);
        foreach (var position in selectedPositions)
        {
            if (!areas.Contains(position))
                return false;
            effectPositions.Add(position);
        }
        return true;
    }
}

public class AreaSkillSelection : SkillSelection
{
    public GridSelection EffectArea { get; set; }
    
    public override bool TryGetEffectPositions(Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        var (areas, splits) = GetSelectablePositions(grid, startPosition, sourceEntity);
        foreach (var position in selectedPositions)
        {
            if (!areas.Contains(position))
                return false;
            effectPositions.UnionWith(EffectArea.GetSelection(grid, position, sourceEntity));
        }
        return true;
    }
}

public class FillSkillSelection : SkillSelection
{
    public bool CombineAreas { get; set; } = false;
    
    public override bool TryGetEffectPositions(Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions, out HashSet<(int, int)> effectPositions, List<int> splitAreaSelections=null)
    {
        effectPositions = new HashSet<(int, int)>();
        if (selectedPositions.Count != SelectionAmount)
            return false;
        if (!CombineAreas && (splitAreaSelections == null || splitAreaSelections.Count != SelectionAmount))
            return false;
        
        var (areas, splits) = GetSelectablePositions(grid, startPosition, sourceEntity);
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

// public class ProjectileSelection : SkillSelection
// {
//     public int ProjectileAmount { get; set; } = 1;
//     public bool SplitSelectionDirections { get; set; } = false;
// }