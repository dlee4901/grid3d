using System.Collections.Generic;

public static partial class UnitConfigs
{
    public static readonly EntityConfig Skeleton = new()
    {
        Id = "Skeleton",
        Cost = 1,
        Health = 1,
        Abilities = new List<string> {"MoveStraight2Step", "Bone Bash", "Undead"}
    };
}
