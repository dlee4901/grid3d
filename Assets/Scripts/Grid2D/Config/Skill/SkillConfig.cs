using System.Collections.Generic;

public class SkillConfig : INameId
{
    public string Id { get; set; }
    public int Cost { get; set; } = 1;
    public int InitialCooldown { get; set; } = 0;
    public int CastTime { get; set; } = 0;
    public int Duration { get; set; } = 0;
    public int Cooldown { get; set; } = 0;
    public bool Locked { get; set; } = false;
    public List<SkillBindings> Bindings { get; set; }
    public SkillSelection Selection { get; set; }
}

public class SkillBindings
{
    List<SkillTrigger> Triggers { get; set; }
    List<SkillEffect> Effects { get; set; }
}