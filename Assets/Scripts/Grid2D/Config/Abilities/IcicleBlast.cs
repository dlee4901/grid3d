using System.Collections.Generic;

public static partial class AbilityConfigs
{
    public static readonly AbilityConfig IcicleBlast = new()
    {
        Id = "IcicleBlast",
        Selection = new FillAbilitySelection()
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
                            MaxDistance = 6
                        }
                    }
                },
                new GridSelection()
                {
                    Traversals = new List<GridTraversal>()
                    {
                        new GridTraversal()
                        {
                            Direction = DirectionType.South,
                            MaxDistance = 6
                        }
                    }
                }
            }
        }
    };
}
