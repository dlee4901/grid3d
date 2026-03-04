#nullable enable

using System.Collections.Generic;

public class Entity : INameId
{
    public string Id { get; }
    public int Cost { get; } // -1: map, 0: summons, 1~n: units
    
    public DirectionFacing Facing { get; private set; } = DirectionFacing.North;
    
    public List<Skill> Skills { get; }
    public HealthComponent? Health { get; }
    public ControlComponent? Control { get; }
    
    static Entity()
    {
        AccessorRegistry<Entity>.Register<string>("Id", e => e.Id);
        AccessorRegistry<Entity>.Register<int>("Cost", e => e.Cost);
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
    
    internal void SetFacing(DirectionFacing facing)
    {
        Facing = facing;
    }
    
    // private readonly List<IComponent> _components = new();
    
    // public void AddComponent(IComponent component) => _components.Add(component);
    //
    // public bool TryGetComponent<T>(out T component) where T : class, IComponent
    // {
    //     component = _components.OfType<T>().FirstOrDefault();
    //     return component != null;
    // }
    //
    // public IEnumerable<T> GetAllComponents<T>() where T : class, IComponent => _components.OfType<T>();


    // public int Health;
    // public List<StatusEffect> StatusEffects;
    //
    // public int PlayerController;
    // public DirectionFacing DirectionFacing;
    //
    // public bool HasSameController(Entity entity)
    // {
    //     return PlayerController == entity.PlayerController;
    // }
}