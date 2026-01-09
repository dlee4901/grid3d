#nullable enable

using Newtonsoft.Json;

public enum DirectionFacing {North, East, South, West}

public class Entity
{
    public string Name { get; }
    public int Cost { get; } // -1: map, 0: summons, 1~n: units
    
    public DirectionFacing Facing { get; private set; } = DirectionFacing.North;
    
    public SkillComponent? Skills { get; }
    public HealthComponent? Health { get; }
    public ControlComponent? Control { get; }
    public MoveComponent? Move { get; }
    
    static Entity()
    {
        AccessorRegistry<Entity>.Register<string>("Name", e => e.Name);
        AccessorRegistry<Entity>.Register<int>("Cost", e => e.Cost);
    }
    
    private Entity(string name, int cost, SkillComponent? skills, HealthComponent? health, ControlComponent? control, MoveComponent? move)
    {
        Name = name;
        Cost = cost;
        Skills = skills;
        Health = health;
        Control = control;
        Move = move;
    }
    
    public static Entity Create(EntityConfig config)
    {
        return new Entity(
            name: config.Name,
            cost: config.Cost,
            health: config.Health > 0 ? new HealthComponent(config.Health) : null,
            skills: null,
            control: null,
            move: null
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