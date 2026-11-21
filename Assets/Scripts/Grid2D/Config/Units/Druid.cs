using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Druid = new()
    {
        Id = "Druid",
        Health = 10,
        Abilities = new List<string>()
    };
}
