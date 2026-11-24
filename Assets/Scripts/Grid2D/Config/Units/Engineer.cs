using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Engineer = new()
    {
        Id = "Engineer",
        Cost = 5,
        Health = 4,
        Abilities = new List<string> {"MoveStraight3Step", "Build Turret", "Upgrade Turret"}
    };
}
