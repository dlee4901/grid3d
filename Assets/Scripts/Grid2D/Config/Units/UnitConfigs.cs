using System.Collections.Generic;

public static class UnitConfigs
{
    public static readonly EntityConfig IceWizard = new()
    {
        Id = "IceWizard",
        Cost = 4,
        Health = 5,
        Abilities = new List<string> {"MoveStraight3Step", "IcicleBlast"}
    };
}