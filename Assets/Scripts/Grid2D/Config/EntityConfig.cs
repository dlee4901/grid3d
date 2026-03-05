using System.Collections.Generic;

public class EntityConfig : INameId
{
    public string Id { get; set; }
    public int Cost { get; set; } = 0;
    public int Health { get; set; } = 0;
    
    public List<MoveSkillConfig> MoveSkills { get; set; }
    public List<SkillConfig> Skills { get; set; }
}
