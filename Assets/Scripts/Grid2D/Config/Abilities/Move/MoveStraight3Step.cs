using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig MoveStraight3Step = new()
    {
        Id = "MoveStraight3Step",
        Type = AbilityType.Move,
        Targeting = new PositionTargeting()
        {
            SelectableArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = DirectionType.Straight,
                        MaxDistance = 3,
                        Linear = false
                    }
                }
            }
        }
        // TargetingOld = new SingleAbilityTargetingOld()
        // {
        //     SelectableAreas = new List<GridSelection>()
        //     {
        //         new GridSelection()
        //         {
        //             MinDistance = 1,
        //             Traversals = new List<GridTraversal>()
        //             {
        //                 new GridTraversal()
        //                 {
        //                     Direction = DirectionType.Straight,
        //                     MaxDistance = 3,
        //                     Linear = false
        //                 }
        //             }
        //         }
        //     }
        // }
    };
}
