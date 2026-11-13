using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Knight = new()
    {
        Id = "Knight",
        Cost = 3,
        Health = 10,
        Abilities = new List<string> {"MoveStraight3Step", "Sword Slash", "Shield Bash"},
        Traits = new List<string> {"Armored", "Retaliate"}
    };
}
