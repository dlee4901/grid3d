using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveSlip = new()
    {
        Id = "MoveSlip",
        DisplayName = "Slip",
        Description = "Move 3 squares, passing through units.",
        Effects = new List<AbilityEffect>
        {
            new MoveEffect()
        },
        Targeting = OrthogonalSteps(3, Occupant.Any)
    };
}
