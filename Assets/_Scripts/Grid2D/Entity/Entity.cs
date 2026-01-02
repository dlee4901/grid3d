using System.Collections.Generic;
using System.Linq;

public enum DirectionFacing {North, East, South, West}

public class Entity
{
    public string Name { get; }
    private readonly List<IComponent> _components = new();
    
    public Entity(string name)
    {
        Name = name;
    }
    
    public void AddComponent(IComponent component) => _components.Add(component);
    
    public bool TryGetComponent<T>(out T component) where T : class, IComponent
    {
        component = _components.OfType<T>().FirstOrDefault();
        return component != null;
    }
    
    public IEnumerable<T> GetAllComponents<T>() where T : class, IComponent => _components.OfType<T>();


    // public int Health;
    // public List<StatusEffect> StatusEffects;
    //
    public int PlayerController;
    public DirectionFacing DirectionFacing;
    
    public bool HasSameController(Entity entity)
    {
        return PlayerController == entity.PlayerController;
    }
}