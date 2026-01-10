using System.Collections.Generic;

public class SkillConfig
{
    public string Id;
    public int Cost = 1;
    public int InitialCooldown = 0;
    public int CastTime = 0;
    public int Duration = 0;
    public int Cooldown = 1;
    
    public List<Effect> Effects;
    
    public int SelectionAmount = 1;
    public List<TileSelectionBuilder> SelectableAreas;
    public QueryNode SelectionFilter;
    
    public bool EffectEntireSelection = false;
    public TileSelectionBuilder EffectArea;
    public QueryNode EffectFilter;
}