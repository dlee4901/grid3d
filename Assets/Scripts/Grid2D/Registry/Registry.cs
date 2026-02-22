using System;
using System.Collections.Generic;

public interface INameId
{
    string Id { get; }
}

public static class Registry<T> where T : INameId
{
    private static readonly Dictionary<string, T> _items = new();
    
    public static void Register(List<T> items)
    {
        foreach (var item in items)
        {
            Register(item);
        }
    }
    
    private static void Register(T item) 
        => _items.TryAdd(item.Id, item);
    
    public static void Clear() 
        => _items.Clear();

    public static T Get(string id)
        => _items.TryGetValue(id, out var item) ? item : throw new InvalidOperationException($"Item with id '{id}' not registered");
        
    public static int GetCount()
        => _items.Count;
    
    public static string PrintItems()
    {
        var output = "";
        foreach (var kvp in _items)
        {
            output += $"{kvp.Key} ";
        }
        output += "\n";
        return output;
    }
}