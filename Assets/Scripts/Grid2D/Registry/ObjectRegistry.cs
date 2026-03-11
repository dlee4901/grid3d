using System;
using System.Collections.Generic;

public interface INameId
{
    string Id { get; }
}

public static class ObjectRegistry<T> where T : INameId
{
    private static readonly Dictionary<string, T> _objects = new();
    
    public static void Register(List<T> items)
    {
        foreach (var item in items)
        {
            Register(item);
        }
    }
    
    private static void Register(T item) 
        => _objects.TryAdd(item.Id, item);
    
    public static void Clear() 
        => _objects.Clear();

    public static T Get(string id)
        => _objects.TryGetValue(id, out var item) ? item : throw new InvalidOperationException($"Item with id '{id}' not registered");
        
    public static int GetCount()
        => _objects.Count;
    
    public static string PrintItems()
    {
        var output = "";
        foreach (var kvp in _objects)
        {
            output += $"{kvp.Key} ";
        }
        output += "\n";
        return output;
    }
}