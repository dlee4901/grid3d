using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Flameweaver = new()
    {
        Id = "Flameweaver",
        Cost = 4,
        Health = 8,
        Abilities = new List<string> {"MoveStraight3Step", "Fireball", "Flame Wave"}//, "Wildfire"}
    };
}
