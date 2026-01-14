using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

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
    
    public static void Register(T item)
    {
        Register(item.Id, item);
    }
    
    public static void Register(string id, T item)
    {
        if (_items.ContainsKey(id)) throw new InvalidOperationException($"Item with id '{id}' already registered");
        _items[id] = item;
    }

    public static T Get(string id)
        => _items.TryGetValue(id, out var item) ? item : throw new InvalidOperationException($"Item with id '{id}' not registered");
}