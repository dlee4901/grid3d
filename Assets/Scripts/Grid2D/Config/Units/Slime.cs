using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Slime = new()
    {
        Id = "Slime",
        Cost = 2,
        Health = 1,
        Abilities = new List<string> {"MoveStraight2Step", "Slam", "Clone", "Absorb Slime"}
    };
}
