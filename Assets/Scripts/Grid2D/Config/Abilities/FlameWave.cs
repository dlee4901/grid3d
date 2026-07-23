using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig FlameWave = new()
    {
        Id = "Flame Wave",
        Targeting = new FillAbilityTargeting()
        {
            SelectableAreas = new List<GridSelection>()
            {
                new GridSelection()
                {
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.North,
                            MaxDistance = 2,
                            DeltaWidth = 1
                        }
                    }
                }
            }
        }
    };
}