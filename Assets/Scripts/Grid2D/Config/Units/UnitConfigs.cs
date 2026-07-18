using System.Collections.Generic;

public static class UnitConfigs
{
    public static readonly EntityConfig Mage = new()
    {
        Id = "Mage",
        Cost = 4,
        Health = 5,
        Abilities = new List<string> {"MoveStraight3Step", "Spellbook", "IcicleBlast", "Fireball"}
    };
}