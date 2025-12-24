using System;
using System.Collections.Generic;

public enum DirectionFacing {North, East, South, West}

[Serializable]
public class Entity
{
    public int Id;
    public int GridPosition;
    public DirectionFacing DirectionFacing;
    public int PlayerController;
    public int Health;
    public List<StatusEffect> StatusEffects;

    public Entity(int id = 0, int gridPosition = 0, DirectionFacing directionFacing = DirectionFacing.North, int playerController = 0, int health = 0)
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