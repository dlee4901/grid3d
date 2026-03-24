using System.Collections.Generic;

public class SkillComponent : IEntityComponent
{
    public List<Skill> List { get; set; } = new List<Skill>();
    public Dictionary<string, Skill> Dictionary { get; set; } = new Dictionary<string, Skill>();
    
    public SkillComponent(List<string> skillIds)
    {
        foreach (var skillId in skillIds)
        {
            if (!IdRegistry<SkillConfig>.TryGet(skillId, out var config)) continue;
            var skill = Skill.Create(config);
            List.Add(skill);
            Dictionary.Add(skillId, skill);
        }
    }
}