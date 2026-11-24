using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveFly = new()
    {
        Id = "MoveFly",
        DisplayName = "Fly",
        Description = "Move 3 squares, flying over units and ground terrain.",
        Effects = new List<AbilityEffect>
        {
            new MoveEffect { Flying = true }
        },
        Targeting = OrthogonalSteps(3, Occupant.Any)
    };
}
