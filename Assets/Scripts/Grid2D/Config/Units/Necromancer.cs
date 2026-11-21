using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Necromancer = new()
    {
        Id = "Necromancer",
        Health = 10,
        Abilities = new List<string>()
    };
}
