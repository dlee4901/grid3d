public enum AbilityEffectType { ReplaceAbility, AddAbility, Damage, InflictStatus, GainShield }
// public enum Effect {Position, Health, Shield, Damage, Counter}

public class AbilityEffect : TypeConfig
{
    public AbilitySelection Selection { get; set; }
}