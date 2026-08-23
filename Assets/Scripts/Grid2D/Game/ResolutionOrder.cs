using System.Collections.Generic;

public interface IReadOnlyResolutionOrder
{
    IReadOnlyList<IReadOnlyEntity> Entities { get; }
}

public class ResolutionOrder : IReadOnlyResolutionOrder
{
    public IReadOnlyList<IReadOnlyEntity> Entities => _entities;
    
    private List<Entity> _entities = new();
    
    public void Add(Entity entity)
    {
        if (_entities.Contains(entity)) return;
        _entities.Add(entity);
    }
    
    public void Remove(Entity entity)
    {
        _entities.Remove(entity);
    }
    
    public bool Contains(Entity entity)
    {
        return _entities.Contains(entity);
    }
    
    public void MoveToFront(Entity entity)
    {
        Move(entity, 0);
    }
    
    public List<Entity> GetOrderByPriority(HashSet<Entity> subset)
    {
        var ordered = new List<Entity>();
        foreach (var entity in _entities) if (subset.Contains(entity)) ordered.Add(entity);
        return ordered;
    }
    
    private void Move(int oldIndex, int newIndex)
    {
        if (oldIndex < 0 || oldIndex >= _entities.Count || newIndex < 0 || newIndex >= _entities.Count)
        {
            GridLog.Error("index out of bounds");
            return;
        }
        if (oldIndex == newIndex) return;
        var entity = _entities[oldIndex];
        _entities.RemoveAt(oldIndex);
        _entities.Insert(newIndex, entity);
    }
    
    private void Move(Entity entity, int newIndex)
    {
        if (newIndex < 0 || newIndex >= _entities.Count)
        {
            GridLog.Error("index out of bounds");
            return;
        }
        var oldIndex = _entities.IndexOf(entity);
        if (oldIndex < 0)
        {
            GridLog.Error("entity not found");
            return;
        }
        if (oldIndex == newIndex) return;
        _entities.RemoveAt(oldIndex);
        _entities.Insert(newIndex, entity);
    }
}