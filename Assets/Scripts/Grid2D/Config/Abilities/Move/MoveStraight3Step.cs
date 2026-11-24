using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveStraight3Step = new()
    {
        Id = "MoveStraight3Step",
        DisplayName = "Move",
        Description = "Move 3 squares.",
        Effects = new List<AbilityEffect>
        {
            new MoveEffect()
        },
        Targeting = OrthogonalSteps(3)
    };

    private static PositionTargeting OrthogonalSteps(int distance, Occupant passthrough = Occupant.Empty) => new()
    {
        SelectableArea = new GridSelection()
        {
            MinDistance = 1,
            Traversals = new List<GridTraversal>()
            {
                new GridTraversal()
                {
                    Direction = GridDirection.Orthogonal,
                    MaxDistance = distance,
                    Linear = false,
                    Passthrough = passthrough
                }
            }
        }
    };
}
