using System;

public sealed class PredicateBuilder<T>
{
    private Func<T, bool> _predicate;

    private PredicateBuilder(Func<T, bool> predicate)
    {
        _predicate = predicate;
    }
    
    public static PredicateBuilder<T> Match(Func<T, bool> predicate)
        => new PredicateBuilder<T>(predicate);
    
    public PredicateBuilder<T> And(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) && predicate(x);
        return this;
    }
    
    public PredicateBuilder<T> Or(Func<T, bool> predicate)
    {
        var left = _predicate;
        _predicate = x => left(x) || predicate(x);
        return this;
    }
    
    public PredicateBuilder<T> Not()
    {
        var prev = _predicate;
        _predicate = x => !prev(x);
        return this;
    }
    
    public Func<T, bool> Build() => _predicate;
}