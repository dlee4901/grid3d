#nullable enable
using System;
using System.Collections.Generic;

public class Entity : INameId
{
    public string Id { get; }
    public int Cost { get; }
    
    public DirectionFacing Facing { get; private set; } = DirectionFacing.North;
    private Dictionary<Type, IEntityComponent> _components { get; } = new();
    
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
    
    public static Entity Create(EntityConfig config, int playerController)
    {
        var entity = new Entity(config.Id, config.Cost);
        entity.AddComponent(new HealthComponent(config.Health));
        entity.AddComponent(new SkillComponent(config.Skills));
        entity.AddComponent(new ControlComponent(playerController));
        return entity;
    }
    
    public void AddComponent<T>(T component) where T : class, IEntityComponent
        => _components[typeof(T)] = component;
    
    public bool TryGetComponent<T>(out T component) where T : class, IEntityComponent
    {
        var success = _components.TryGetValue(typeof(T), out var value);
        component = (T)value;
        return success;
    }
}