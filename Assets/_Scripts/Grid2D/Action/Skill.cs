using System;

// enum TargetType {Select, Projectile, Beam, AOE}

// Select
// Direction: any
// Passthrough: 

public class Skill
{
    public int Id;
    public bool Usable;
    
    public QueryBuilder<Entity> Trigger;

    public Skill(int id=0)
    {
        Id = id;
    }
}