using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Necromancer = new()
    {
        Id = "Necromancer",
        Cost = 6,
        Health = 5,
        Abilities = new List<string> {"MoveStraight3Step", "Summon Skeleton", "Soul Siphon", "Undead"}
    };
}
