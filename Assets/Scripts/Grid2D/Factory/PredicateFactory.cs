using System;
using System.Linq;

public static class PredicateFactory<T>
{
    public static Func<T, bool> Create(PredicateConfig config)
        => config.Operation switch
        {
            ConditionOperation.Match => Match(config),
            ConditionOperation.And   => And(config),
            ConditionOperation.Or    => Or(config),
            ConditionOperation.Not   => Not(config),
            _ => throw new InvalidOperationException()
        };

    private static Func<T, bool> Match(PredicateConfig config)
        => config.ValueType switch
        {
            ConditionValueType.Int   => Compare(config, config.IntValue),
            ConditionValueType.Bool  => Compare(config, config.BoolValue),
            ConditionValueType.String => Compare(config, config.StringValue),
            _ => throw new InvalidOperationException()
        };

    private static Func<T, bool> Compare<TValue>(
        PredicateConfig config, TValue rhs)
        where TValue : IComparable<TValue>
    {
        var accessor = AccessorRegistry<T>.Get<TValue>(config.Field!);

        return config.Comparison switch
        {
            "==" => x => accessor(x).CompareTo(rhs) == 0,
            "!=" => x => accessor(x).CompareTo(rhs) != 0,
            ">"  => x => accessor(x).CompareTo(rhs) > 0,
            "<"  => x => accessor(x).CompareTo(rhs) < 0,
            ">=" => x => accessor(x).CompareTo(rhs) >= 0,
            "<=" => x => accessor(x).CompareTo(rhs) <= 0,
            _ => throw new InvalidOperationException("Invalid comparison")
        };
    }

    private static Func<T, bool> And(PredicateConfig config)
    {
        var compiled = config.Children!.Select(Create).ToArray();
        return x => compiled.All(p => p(x));
    }

    private static Func<T, bool> Or(PredicateConfig config)
    {
        var compiled = config.Children!.Select(Create).ToArray();
        return x => compiled.Any(p => p(x));
    }

    private static Func<T, bool> Not(PredicateConfig config)
    {
        var inner = Create(config.Children![0]);
        return x => !inner(x);
    }
}