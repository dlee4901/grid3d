using System;

public sealed class ConditionBuilder<T>
{
    private Func<T, bool> _predicate;

    private ConditionBuilder(Func<T, bool> predicate)
    {
        _predicate = predicate;
    }

    // Entry point
    public static ConditionBuilder<T> Match(Func<T, bool> predicate)
        => new ConditionBuilder<T>(predicate);

    // AND
    public ConditionBuilder<T> And(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) && predicate(x);
        return this;
    }

    // OR
    public ConditionBuilder<T> Or(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) || predicate(x);
        return this;
    }

    // NOT
    public ConditionBuilder<T> Not()
    {
        var prev = _predicate;
        _predicate = x => !prev(x);
        return this;
    }

    // Export
    public Func<T, bool> Build() => _predicate;
}