#nullable enable
using System;
using System.Collections.Generic;

public class Entity : INameId
{
    public string Id { get; }
    public int Cost { get; } // -1: map, 0: summons, 1~n: units
    
    private Dictionary<Type, IEntityComponent> _components { get; } = new();
    
    public DirectionFacing Facing { get; private set; } = DirectionFacing.North;
    
    static Entity()
    {
        MemberRegistry<Entity>.Register<string>("Id", e => e.Id);
        MemberRegistry<Entity>.Register<int>("Cost", e => e.Cost);
    }
    
    private Entity(string id, int cost)
    {
        Id = id;
        Cost = cost;
    }
    
    public static Entity Create(EntityConfig config)
    {
        return new Entity(
            id: config.Id,
            cost: config.Cost
        );
    }
    
    public void Add<T>(T component) where T : class, IEntityComponent
        => _components[typeof(T)] = component;
    
    public T Get<T>() where T : class, IEntityComponent
        => (T)_components[typeof(T)];
    
    public bool TryGet<T>(out T component) where T : class, IEntityComponent
    {
        var success = _components.TryGetValue(typeof(T), out var value);
        component = (T)value;
        return success;
    }
}