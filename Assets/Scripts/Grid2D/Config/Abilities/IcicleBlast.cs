using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig IcicleBlast = new()
    {
        Id = "IcicleBlast",
        Targeting = new DirectionTargeting()
        {
            Grouping = new int[] {1, 0, 2, 0, 1, 0, 2, 0},
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Straight,
                        MaxDistance = 4
                    }
                }
            }
        }
        // TargetingOld = new FillAbilityTargetingOld()
        // {
        //     SelectableAreas = new List<GridSelection>()
        //     {
        //         new GridSelection()
        //         {
        //             Traversals = new List<GridTraversal>()
        //             {
        //                 new GridTraversal()
        //                 {
        //                     Direction = DirectionType.North,
        //                     MaxDistance = 6
        //                 }
        //             }
        //         },
        //         new GridSelection()
        //         {
        //             Traversals = new List<GridTraversal>()
        //             {
        //                 new GridTraversal()
        //                 {
        //                     Direction = DirectionType.South,
        //                     MaxDistance = 6
        //                 }
        //             }
        //         }
        //     }
        // }
    };
}
