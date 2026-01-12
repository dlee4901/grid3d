using System;
using System.Collections.Generic;

public static class AccessorRegistry<TTarget>
{
    private static readonly Dictionary<(Type valueType, string name), Delegate> _accessors
        = new();

    public static void Register<TValue>(string name, Func<TTarget, TValue> accessor)
    {
        _accessors[(typeof(TValue), name)] = accessor;
    }

    public static Func<TTarget, TValue> Get<TValue>(string name)
    {
        if (!_accessors.TryGetValue((typeof(TValue), name), out var del))
            throw new InvalidOperationException(
                $"Accessor '{name}' of type '{typeof(TValue).Name}' not registered");

        return (Func<TTarget, TValue>)del;
    }
}