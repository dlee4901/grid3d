public enum GameEvent { TurnEnd, DamageDealt, DamageTaken, Damaged, Died }

public sealed class Trigger
{
    public GameEvent On { get; init; }
    public Occupant Subject { get; init; } = Occupant.Self;
    public GridSelection Range { get; init; }
    public Condition When { get; init; } = Condition.Always;
}
