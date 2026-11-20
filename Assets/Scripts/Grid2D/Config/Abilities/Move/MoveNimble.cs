using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveNimble = new()
    {
        Id = "MoveNimble",
        DisplayName = "Nimble Move",
        Description = "Move 3 squares.",
        Keywords = Keyword.Finisher | Keyword.Double,
        Effects = new List<AbilityEffect> { new MoveEffect() },
        Targeting = Straight3Step()
    };
}
