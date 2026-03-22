using System.Collections.Generic;

public abstract class SkillSelection
{
    public List<GridSelection> SelectableAreas { get; set; }
    public PredicateConfig Filter { get; set; }
    
    public List<HashSet<Step>> GetSelectableSteps(Grid2D grid, (int, int) startPosition, Entity sourceEntity)
    {
        var selectableSteps = new List<HashSet<Step>>();
        foreach (var gridSelection in SelectableAreas)
            selectableSteps.Add(gridSelection.GetSteps(grid, startPosition, sourceEntity));
        return selectableSteps;
    }
    
    public List<HashSet<(int, int)>> GetSelectableAreas(Grid2D grid, (int, int) startPosition, Entity sourceEntity)
    {
        var selectableSteps = new List<HashSet<(int, int)>>();
        foreach (var gridSelection in SelectableAreas)
            selectableSteps.Add(gridSelection.GetSelection(grid, startPosition, sourceEntity));
        return selectableSteps;
    }
    
    public HashSet<(int, int)> GetSelectableAreasCombined(Grid2D grid, (int, int) startPosition, Entity sourceEntity)
    {
        var selectableAreasCombined = new HashSet<(int, int)>();
        foreach (var gridSelection in SelectableAreas)
            foreach (var step in gridSelection.GetSteps(grid, startPosition, sourceEntity))
                selectableAreasCombined.Add(step.Position);
        return selectableAreasCombined;
    }
}

public class SingleSkillSelection : SkillSelection
{
    public int SelectionAmount { get; set; } = 1;
    
    public bool Evaluate(out List<(int, int)> selectedAreas, Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions)
    {
        selectedAreas = new List<(int, int)>();
        for (var i = 0; i < SelectionAmount; i++)
        {
            if (selectedPositions.Count < SelectionAmount || !GetSelectableAreasCombined(grid, startPosition, sourceEntity).Contains(selectedPositions[i]))
                return false;
            selectedAreas.Add(selectedPositions[i]);
        }
        return true;
    }
}

public class AreaSkillSelection : SkillSelection
{
    public GridSelection EffectArea { get; set; }
    
    public bool Evaluate(out HashSet<(int, int)> selectedAreas, Grid2D grid, (int, int) startPosition, Entity sourceEntity, (int, int) selectedPosition)
    {
        selectedAreas = new HashSet<(int, int)>();
        if (!GetSelectableAreasCombined(grid, startPosition, sourceEntity).Contains(selectedPosition))
            return false;
        selectedAreas = EffectArea.GetSelection(grid, selectedPosition, sourceEntity);
        return true;
    }
}

public class FillSkillSelection : SkillSelection
{
    public bool CombineAreas { get; set; } = false;
    
    public bool Evaluate(out HashSet<(int, int)> selectedAreas, Grid2D grid, (int, int) startPosition, Entity sourceEntity, (int, int) selectedPosition, int selectedIndex=-1)
    {
        selectedAreas = new HashSet<(int, int)>();
        if (selectedIndex >= 0 && selectedIndex < SelectableAreas.Count)
        {
            selectedAreas = SelectableAreas[selectedIndex].GetSelection(grid, startPosition, sourceEntity);
            return true;
        }
        foreach (var selectableArea in SelectableAreas)
        {
            var selection = selectableArea.GetSelection(grid, startPosition, sourceEntity);
            if (!selection.Contains(selectedPosition)) continue;
            selectedAreas = selection;
            return true;
        }
        return false;
    }
}

public class ProjectileSelection : SkillSelection
{
    public int SelectionAmount { get; set; } = 1;
    public int ProjectileAmount { get; set; } = 1;
    public bool SplitSelectionDirections { get; set; } = false;
}