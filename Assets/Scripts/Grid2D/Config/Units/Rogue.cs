using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Rogue = new()
    {
        Id = "Rogue",
        Cost = 2,
        Health = 7,
        Abilities = new List<string> {"MoveStraight3Step", "Knife Stab", "Crossbow Shot", "Nimble Cloak"}
    };
}
