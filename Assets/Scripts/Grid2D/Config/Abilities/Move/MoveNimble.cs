using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveNimble = new()
    {
        Id = "MoveNimble",
        Keywords = Keyword.Successive | Keyword.Repeat,
        Effects = new List<AbilityEffect> { new MoveEffect() },
        Targeting = Straight3Step()
    };
}
