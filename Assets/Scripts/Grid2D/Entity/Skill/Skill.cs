using System.Collections.Generic;

public class Skill : INameId
{
    public string Id { get; }
    public int Cost { get; }
    public int Warmup { get; }
    public int CastTime { get; }
    public int Duration { get; }
    public int Cooldown { get; }
    public bool Locked { get; }
    public SkillSelection Selection { get; }
    public List<SkillBindings> Bindings { get; }
    
    static Skill()
    {
        MemberRegistry<Skill>.Register<string>("Id", e => e.Id);
        MemberRegistry<Skill>.Register<int>("Cost", e => e.Cost);
    }
    
    private Skill(string id, int cost, int warmup, int castTime, int duration, int cooldown, bool locked, SkillSelection selection)
    {
        Id = id;
        Cost = cost;
        Warmup = warmup;
        CastTime = castTime;
        Duration = duration;
        Cooldown = cooldown;
        Locked = locked;
        Selection = selection;
    }
    
    public static Skill Create(SkillConfig config)
    {
        var skill = new Skill(config.Id, config.Cost, config.Warmup, config.CastTime, config.Duration, config.Cooldown, config.Locked, config.Selection);
        return skill;
    }
}