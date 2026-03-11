using System.Collections.Generic;

public class SkillSelection
{
    public List<GridSelection> SelectableAreas { get; set; }
}

public class SingleSkillSelection : SkillSelection
{
    public int SelectionAmount { get; set; } = 1;
}

public class AreaSkillSelection : SkillSelection
{
    public int SelectionAmount { get; set; } = 1;
    public GridSelection EffectArea { get; set; }
}

public class FillSkillSelection : SkillSelection
{
    public bool CombineAreas { get; set; } = false;
}