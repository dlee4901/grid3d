public enum AbilityTriggerType { OnSelect, OnTurnStart, OnDeath, OnKill, OnHealthDown, OnMove }

public class AbilityTrigger : TypeConfig
{
    public AbilityTargeting Targeting { get; set; }
}