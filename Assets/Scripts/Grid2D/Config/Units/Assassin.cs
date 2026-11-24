using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Assassin = new()
    {
        Id = "Assassin",
        Cost = 3,
        Health = 3,
        Abilities = new List<string> {"MoveSlip", "Backstab", "Mark for Death"}
    };
}
