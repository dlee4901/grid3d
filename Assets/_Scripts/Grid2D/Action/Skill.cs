using System;
using System.Collections.Generic;

// Select
// Direction: any
// Passthrough:

// TileSelectors: Range, Selectable, Affected (Area, Wave, Projectile)
// Num Selections
// 

public class Skill
{
    public int Id;
    
    List<TileSelector> Range;
    List<TileSelector> Targetable;
    
    
    
    public QueryBuilder<Entity> Trigger;

    public Skill(int id=0)
    {
        Id = id;
    }
}