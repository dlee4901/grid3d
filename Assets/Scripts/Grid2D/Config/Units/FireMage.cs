using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig FireMage = new()
    {
        Id = "Fire Mage",
        Cost = 4,
        Health = 12,
        Abilities = new List<string> {"MoveStraight3Step", "Fireball", "Heatwave", "Flame Burst"}//, "Wildfire"}
    };
}
