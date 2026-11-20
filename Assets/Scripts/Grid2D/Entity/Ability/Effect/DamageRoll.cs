using System.Collections.Generic;

public sealed class DamageRoll
{
    private readonly List<(string Source, int Delta)> _modifiers = new();

    public int Amount { get; internal set; }
    public int BaseAmount { get; }
    public bool Prevented { get; internal set; }
    public string Cause { get; }
    public bool FromAbility { get; }
    public IReadOnlyList<(string Source, int Delta)> Modifiers => _modifiers;

    internal DamageRoll(int amount, string cause, bool fromAbility)
    {
        Amount = amount;
        BaseAmount = amount;
        Cause = cause;
        FromAbility = fromAbility;
    }

    internal void Modify(string source, int delta)
    {
        Amount += delta;
        _modifiers.Add((source, delta));
    }
}
