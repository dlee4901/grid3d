using System;

public sealed class QueryBuilder<T>
{
    private Func<T, bool> _predicate;

    private QueryBuilder(Func<T, bool> predicate)
    {
        _predicate = predicate;
    }

    // Entry point
    public static QueryBuilder<T> Match(Func<T, bool> predicate)
        => new QueryBuilder<T>(predicate);

    // AND
    public QueryBuilder<T> And(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) && predicate(x);
        return this;
    }

    // OR
    public QueryBuilder<T> Or(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) || predicate(x);
        return this;
    }

    // NOT
    public QueryBuilder<T> Not()
    {
        var prev = _predicate;
        _predicate = x => !prev(x);
        return this;
    }

    // Export
    public Func<T, bool> Build() => _predicate;
}