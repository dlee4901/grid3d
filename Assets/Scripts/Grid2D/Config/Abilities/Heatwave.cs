using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig Heatwave = new()
    {
        Id = "Heatwave",
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
    };
}