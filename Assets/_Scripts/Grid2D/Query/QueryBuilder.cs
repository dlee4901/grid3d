using System;

public sealed class QueryBuilder<T>
{
    private Func<T, bool> _predicate;

    private QueryBuilder(Func<T, bool> predicate)
    {
        _predicate = predicate;
    }
    
    public static QueryBuilder<T> Match(Func<T, bool> predicate)
        => new QueryBuilder<T>(predicate);
    
    public QueryBuilder<T> And(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) && predicate(x);
        return this;
    }
    
    public QueryBuilder<T> Or(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) || predicate(x);
        return this;
    }
    
    public QueryBuilder<T> Not()
    {
        var prev = _predicate;
        _predicate = x => !prev(x);
        return this;
    }
    
    public Func<T, bool> Build() => _predicate;
}