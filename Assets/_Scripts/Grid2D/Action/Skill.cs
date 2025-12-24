using System;

// enum TargetType {Select, Projectile, Beam, AOE}

// Select
// Direction: any
// Passthrough: 

[Serializable]
public class Skill
{
    public int Id { get; private set; }

    public Skill(int id=0)
    {
        Id = id;
    }
}