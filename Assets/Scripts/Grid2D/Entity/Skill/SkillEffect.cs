public enum SkillEffectType { ReplaceSkill, AddSkill, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

public class SkillEffect : TypeConfig
{
    public SkillSelection Selection { get; set; }
}