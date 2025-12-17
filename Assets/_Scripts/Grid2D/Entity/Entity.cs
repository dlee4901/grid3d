using System;
using System.Collections.Generic;

public enum DirectionFacing {North, East, South, West}

[Serializable]
public class Entity
{
    public int Id { get; private set; }
    public int GridPosition { get; private set; }
    public DirectionFacing DirectionFacing { get; private set; }
    public int PlayerController { get; private set; }
    public int Health { get; private set; }
    public List<StatusEffect> StatusEffects { get; private set; }

    public Entity(int id = 0, int gridPosition = 0, DirectionFacing directionFacing = global::DirectionFacing.North, int playerController = 0, int health = 0)
    {
        Id = id;
        GridPosition = gridPosition;
        DirectionFacing = directionFacing;
        PlayerController = playerController;
        Health = health;
        StatusEffects = new List<StatusEffect>();
    }

    public bool HasSameController(Entity entity)
    {
        return PlayerController == entity.PlayerController;
    }
}