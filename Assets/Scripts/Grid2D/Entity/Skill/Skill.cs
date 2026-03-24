using System.Collections.Generic;

public class Skill : INameId
{
    public string Id { get; set; }
    public int Cost { get; set; }
    public int Warmup { get; set; }
    public int CastTime { get; set; }
    public int Duration { get; set; }
    public int Cooldown { get; set; }
    public bool Locked { get; set; }
    public List<SkillBindings> Bindings { get; set; }
    public SkillSelection Selection { get; set; }
    
    static Skill()
    {
        MemberRegistry<Skill>.Register<string>("Id", e => e.Id);
        MemberRegistry<Skill>.Register<int>("Cost", e => e.Cost);
    }
    
    private Skill(string id, int cost, int warmup, int castTime, int duration, int cooldown, bool locked)
    {
        Id = id;
        Cost = cost;
        Warmup = warmup;
        CastTime = castTime;
        Duration = duration;
        Cooldown = cooldown;
        Locked = locked;
        Bindings = new List<SkillBindings>();
    }
    
    public static Skill Create(SkillConfig config)
    {
        var skill = new Skill(config.Id, config.Cost, config.Warmup, config.CastTime, config.Duration, config.Cooldown, config.Locked);
        return skill;
    }
}