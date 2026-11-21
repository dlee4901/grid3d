using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Engineer = new()
    {
        Id = "Engineer",
        Health = 10,
        Abilities = new List<string>()
    };
}
