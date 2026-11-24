using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveStraight2Step = new()
    {
        Id = "MoveStraight2Step",
        DisplayName = "Move",
        Description = "Move 2 squares.",
        Effects = new List<AbilityEffect>
        {
            new MoveEffect()
        },
        Targeting = OrthogonalSteps(2)
    };
}
