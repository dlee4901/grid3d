using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig FlameWave = new()
    {
        Id = "Flame Wave",
        Targeting = new DirectionTargeting()
        {
            Grouping = new [] {1, 0, 2, 0, 3, 0, 4, 0},
            EffectArea = new GridSelection()
            {
                Traversals = new List<GridTraversal>()
                {
                    new GridTraversal()
                    {
                        Direction = GridDirection.Straight,
                        MaxDistance = 2,
                        DeltaWidth = 1
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
        //                     MaxDistance = 2,
        //                     DeltaWidth = 1
        //                 }
        //             }
        //         }
        //     }
        // }
    };
}