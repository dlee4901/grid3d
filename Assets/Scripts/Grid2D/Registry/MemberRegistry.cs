using System;
using System.Collections.Generic;

public static class MemberRegistry<TClass>
{
    private static readonly Dictionary<(Type valueType, string name), Delegate> _members = new();

    public static void Register<TMember>(string name, Func<TClass, TMember> accessor) 
        => _members[(typeof(TMember), name)] = accessor;

    public static Func<TClass, TMember> Get<TMember>(string name)
    {
        if (!_members.TryGetValue((typeof(TMember), name), out var del))
            throw new InvalidOperationException($"Member '{name}' of type '{typeof(TMember).Name}' not registered");

        return (Func<TClass, TMember>)del;
    }
}