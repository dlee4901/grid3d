using System;
using System.Collections.Generic;

public static class TypeRegistry
{
    private static readonly Dictionary<string, Type> _types = new();
    
    public static void Register(Type type)
        => _types.TryAdd(type.Name, type);
        
    public static bool TryGetValue(string typeName, out Type type)
        => _types.TryGetValue(typeName, out type);
}