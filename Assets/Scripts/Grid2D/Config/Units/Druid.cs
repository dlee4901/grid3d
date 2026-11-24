using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Druid = new()
    {
        Id = "Druid",
        Cost = 5,
        Health = 6,
        Abilities = new List<string> {"MoveStraight3Step", "Summon Thorns", "Summon Bat"}
    };
}
