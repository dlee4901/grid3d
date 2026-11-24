using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveNimble = new()
    {
        Id = "MoveNimble",
        DisplayName = "Nimble Move",
        Description = "Move 4 squares.",
        Effects = new List<AbilityEffect>
        {
            new MoveEffect()
        },
        Targeting = OrthogonalSteps(4)
    };
}
