#nullable enable
using System;
using System.Collections.Generic;

public class Entity : INameId
{
    public string Id { get; }
    public int Uid { get; internal set; }
    public int Cost { get; }
    
    public GridPosition Position { get; private set; }
    public GridDirectionFacing Facing { get; private set; } = GridDirectionFacing.North;
    private Dictionary<Type, IEntityComponent> _components { get; } = new();
    
    private Entity(string id, int cost)
    {
        Id = id;
        Cost = cost;
    }
    
    public static Entity Create(EntityConfig config, int playerController)
    {
        var entity = new Entity(config.Id, config.Cost)
        {
            Facing = playerController == 2 ? GridDirectionFacing.South : GridDirectionFacing.North
        };
        entity.AddComponent(new HealthComponent(config.Health));
        entity.AddComponent(new AbilityComponent(config.Abilities));
        entity.AddComponent(new ControlComponent(playerController));
        entity.AddComponent(new StatusComponent(config.Statuses));
        return entity;
    }
    
    internal Entity Copy()
    {
        var copy = new Entity(Id, Cost) { Facing = Facing };
        foreach (var (type, component) in _components) copy._components[type] = component.Copy();
        return copy;
    }

    internal void AddComponent<T>(T component) where T : class, IEntityComponent
        => _components[typeof(T)] = component;
    
    public bool TryGetComponent<T>(out T component) where T : class, IEntityComponent
    {
        var success = _components.TryGetValue(typeof(T), out var value);
        component = (T)value;
        return success;
    }
    
    internal void SetPosition(GridPosition position)
    {
        Position = position;
    }
}