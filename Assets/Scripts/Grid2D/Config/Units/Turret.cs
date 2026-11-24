using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Turret = new()
    {
        Id = "Turret",
        Cost = 0,
        Health = 3,
        Abilities = new List<string> {"Shoot", "Upgrade Level"}
    };
}
