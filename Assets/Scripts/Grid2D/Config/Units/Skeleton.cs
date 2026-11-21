using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Skeleton = new()
    {
        Id = "Skeleton",
        Health = 10,
        Abilities = new List<string>()
    };
}
