using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Knight = new()
    {
        Id = "Knight",
        Cost = 3,
        Health = 10,
        Abilities = new List<string> {"MoveKnight", "Sword Slash", "Shield Raise", "Knight's Armor"}
    };
}
