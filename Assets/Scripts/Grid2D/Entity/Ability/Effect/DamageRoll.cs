public sealed class DamageRoll
{
    public int Amount { get; internal set; }
    public bool Prevented { get; internal set; }
    public string Cause { get; }
    public bool FromAbility { get; }

    internal DamageRoll(int amount, string cause, bool fromAbility)
    {
        Amount = amount;
        Cause = cause;
        FromAbility = fromAbility;
    }
}
