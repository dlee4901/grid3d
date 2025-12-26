using System;
using System.Collections.Generic;

public enum DirectionFacing {North, East, South, West}

public class Entity
{
    public int Id;
    
    public int Health;
    public List<StatusEffect> StatusEffects;
    
    public int PlayerController;
    public DirectionFacing DirectionFacing;
    

    public Entity(int id, int gridPosition, DirectionFacing directionFacing, int playerController, int health)
    {
        Id = id;
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