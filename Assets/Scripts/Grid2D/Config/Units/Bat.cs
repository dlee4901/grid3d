using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Bat = new()
    {
        Id = "Bat",
        Cost = 1,
        Health = 1,
        Abilities = new List<string> {"MoveFly", "Rabies Bite"}
    };
}
