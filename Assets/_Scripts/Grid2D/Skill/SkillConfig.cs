using System.Collections.Generic;

public class SkillConfig
{
    public string Name;
    public int CastTime;
    public int Cooldown;
    
    public QueryNode Conditions;
    public List<Effect> Effects;
    
    public List<TileSelectionBuilder> SelectableAreas;
    public TileSelectionBuilder EffectArea;
    public QueryNode SelectionQuery;
}