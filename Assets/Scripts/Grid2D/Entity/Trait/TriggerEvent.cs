public sealed class TriggerEvent
{
    public GameEvent On { get; }
    public Entity Subject { get; }
    public Entity Other { get; }
    public int Distance { get; }
    public DamageRoll Damage { get; }

    internal TriggerEvent(GameEvent on, Entity subject, Entity other = null, int distance = 0, DamageRoll damage = null)
    {
        On = on;
        Subject = subject;
        Other = other;
        Distance = distance;
        Damage = damage;
    }
}
