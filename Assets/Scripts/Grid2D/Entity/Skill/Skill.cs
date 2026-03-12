public class Skill : INameId
{
    public string Id { get; set; }
    public int Cost { get; set; }
    public SkillSelection Selection { get; set; }
}