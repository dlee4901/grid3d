using System.Collections.Generic;

public class SkillConfig : INameId
{
    public string Id { get; set; }
    public int Cost { get; set; } = 1;
    public int InitialCooldown { get; set; } = 0;
    public int CastTime { get; set; } = 0;
    public int Duration { get; set; } = 0;
    public int Cooldown { get; set; } = 1;
    
    public List<Effect> Effects { get; set; }
    
    public int SelectionAmount { get; set; } = 1;
    public List<TileSelectionBuilder> SelectableAreas { get; set; }
    public PredicateConfig SelectionFilter { get; set; }
    
    public bool EffectEntireSelection { get; set; } = false;
    public TileSelectionBuilder EffectArea { get; set; }
    public PredicateConfig EffectFilter { get; set; }
}