using System;
using System.Collections.Generic;

public interface INameId
{
    string Id { get; }
}

public static class IdRegistry<T> where T : INameId
{
    private static readonly Dictionary<string, T> _items = new();
    
    public static void Register(List<T> items)
    {
        foreach (var item in items)
        {
            Register(item);
        }
    }
    
    public static void Register(T item) 
        => _items.TryAdd(item.Id, item);
    
    public static void Clear() 
        => _items.Clear();

    public static bool TryGet(string id, out T item)
        => _items.TryGetValue(id, out item);
        
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