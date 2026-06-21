public enum AbilityTriggerType { OnSelect, OnTurnStart, OnDeath, OnKill, OnHealthDown, OnMove }

public class AbilityTrigger : TypeConfig
{
    public AbilitySelection Selection { get; set; }
}