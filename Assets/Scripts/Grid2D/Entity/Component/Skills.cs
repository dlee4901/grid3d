using System.Collections.Generic;

public class Skills : IEntityComponent
{
    public List<Skill> List { get; set; }
    public Dictionary<string, Skill> Dictionary { get; set; }
}