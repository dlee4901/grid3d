using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Barbarian = new()
    {
        Id = "Barbarian",
        Cost = 1,
        Health = 9,
        Abilities = new List<string> {"MoveStraight3Step", "Axe Swing", "Ham Snack"}
    };
}
