public enum SkillTriggerType { OnSelect, OnTurnStart, OnDeath, OnKill, OnHealthDown, OnMove }

public class SkillTrigger : TypeConfig
{
    public SkillSelection Selection { get; set; }
}