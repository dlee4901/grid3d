using System.Collections.Generic;

public abstract class SkillSelection
{
    public List<GridSelection> SelectableAreas { get; set; }
    
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
    
    public bool Evaluate(Grid2D grid, (int, int) startPosition, Entity sourceEntity, List<(int, int)> selectedPositions, out List<(int, int)> selectedAreas)
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
    
    public bool Evaluate(Grid2D grid, (int, int) startPosition, Entity sourceEntity, (int, int) selectedPosition, out HashSet<(int, int)> selectedAreas)
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
    
    public bool Evaluate(Grid2D grid, (int, int) startPosition, Entity sourceEntity, (int, int) selectedPosition, out HashSet<(int, int)> selectedAreas)
    {
        //var selectableAreas = GetSelectableAreas(grid, startPosition, sourceEntity);
        
    }
}

public class ProjectileSelection : SkillSelection
{
    public int SelectionAmount { get; set; } = 1;
    public int ProjectileAmount { get; set; } = 1;
    public bool SplitSelectionDirections { get; set; } = false;
}