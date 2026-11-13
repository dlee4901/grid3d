using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveStraight3Step = new()
    {
        Id = "MoveStraight3Step",
        Effects = new List<AbilityEffect> { new MoveEffect() },
        Targeting = new PositionTargeting()
        {
            SelectableArea = new GridSelection()
            {
                MinDistance = 1,
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Straight,
                        MaxDistance = 3,
                        Linear = false
                    }
                }
            }
        }
    };
}
